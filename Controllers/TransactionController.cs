using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SettleDown.CRUDs;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Helpers;
using SettleDown.Models;
using SettleDown.Services;

namespace SettleDown.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly SettleDownContext _context;
        private readonly ICrudService<SettleDownMember> _memberCrudService;
        private readonly IDebtManagementService _debtManagementService;
        
        private const string JwtMemberClaimDelimiter = ";";
        
        
        public TransactionController(SettleDownContext context, ICrudService<SettleDownMember> memberCrudService,
            IDebtManagementService debtManagementService)
        {
            _context = context;
            _memberCrudService = memberCrudService;
            _debtManagementService = debtManagementService;
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettleDownTransaction>>> GetSettleDownTransaction()
        {
            if (_context.SettleDownTransaction == null)
            {
                return NotFound();
            }

            string[] memberClaims = User.Claims.First(c => c.Type == "members").Value
                .Split(JwtMemberClaimDelimiter);

            if (memberClaims[0].IsNullOrEmpty())
                return new List<SettleDownTransaction>();

            IEnumerable<SettleDownTransaction> allTransactions = new List<SettleDownTransaction>();

            foreach (string claim in memberClaims)
            {
                SettleDownMember? member = await GetMemberFromClaim(claim);
                if (member == null)
                    continue;

                allTransactions = allTransactions.Concat(await GetTransactionsForMember(member));
                
            }
            
            return allTransactions.ToList();
        }

        private async Task<IEnumerable<SettleDownTransaction>> GetTransactionsForMember(SettleDownMember member)
        {
            return await (_context.SettleDownTransaction ??
                          throw new InvalidOperationException("Entity set 'SettleDownContext.SettleDownTransaction'  is null."))
                .Where(t => t.MemberId == member.Id).ToListAsync();
        }

        private async Task<SettleDownMember?> GetMemberFromClaim(string claim)
        {
            if(int.TryParse(claim, out int id))
                return await _context.SettleDownMember!.FindAsync(id);
            return null;
        }

        // GET: api/Transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SettleDownTransaction>> GetSettleDownTransaction(int id)
        {
            if (_context.SettleDownTransaction == null)
            {
                return NotFound();
            }
            
            var settleDownTransaction = await _context.SettleDownTransaction.FindAsync(id);
            
            if (settleDownTransaction == null)
            {
                return NotFound();
            }

            if (!HasMemberClaimToTransaction(settleDownTransaction.MemberId) && 
                !HasGroupClaimToTransaction(settleDownTransaction.GroupId))
                return Unauthorized("You do not have access to this transaction!");
            
            return settleDownTransaction;
        }

        private bool HasMemberClaimToTransaction(int? id)
        {
            if (id == null)
                return false;
            
            string[] memberClaims = User.Claims.First(c => c.Type == "members").Value
                .Split(JwtMemberClaimDelimiter);

            return memberClaims.Contains(id.ToString());
        }

        private bool HasGroupClaimToTransaction(int? id)
        {
            if (id == null)
                return false;
            
            string[] groupClaims = User.Claims.First(c => c.Type == "groups").Value
                .Split(JwtMemberClaimDelimiter);

            return groupClaims.Contains(id.ToString());
        }

        // PUT: api/Transaction/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSettleDownTransaction(int id, SettleDownTransactionPutDto dto)
        {
            IActionResult? authorizeAndValidatePutRequest = await AuthorizeAndValidatePutRequest(id);
            if (authorizeAndValidatePutRequest != null)
            {
                return authorizeAndValidatePutRequest;
            }
            
            SettleDownTransaction settleDownTransaction = (await _context.SettleDownTransaction!.FindAsync(id))!;
            settleDownTransaction.Name = dto.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<IActionResult?> AuthorizeAndValidatePutRequest(int id)
        {
            if (_context.SettleDownTransaction == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownTransaction'  is null.");
            }

            SettleDownTransaction? originalTransaction = await _context.SettleDownTransaction.FindAsync(id);

            if (originalTransaction == null)
            {
                return NotFound("No such transaction exists! Use POST to create entries.");
            }

            if (!HasMemberClaimToTransaction(originalTransaction.MemberId) &&
                !HasGroupClaimToTransaction(originalTransaction.GroupId))
                return Unauthorized("You do not have access to this transaction!");
            
            return null;
        }

        // POST: api/Transaction
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SettleDownTransaction>> PostSettleDownTransaction(SettleDownTransactionDto dto)
        {
            ActionResult<SettleDownTransaction>? actionResult = AuthorizePostSettleDownTransaction(dto);
            if (actionResult != null) 
                return actionResult;

            try
            {
                if (dto.MemberId == null)
                {
                    actionResult = await PopulateDtoWithMemberGeneratedFromCurrentUser(dto);
                    if (actionResult != null)
                    {
                        DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                        return actionResult;
                    }
                }

                if (_context.SettleDownTransaction == null)
                {
                    DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                    return Problem("Entity set 'SettleDownContext.SettleDownTransaction'  is null.");
                }

                SettleDownTransaction settleDownTransaction = InitializeTransactionFromDto(dto);
                _context.SettleDownTransaction.Add(settleDownTransaction);

                ActionResult? disperseDebtFromTransaction =
                    await _debtManagementService.DisperseDebtFromTransaction(dto);
                if (disperseDebtFromTransaction != null)
                {
                    DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                    return disperseDebtFromTransaction;
                }

                ActionResult? updateTotalBalanceForMembers = await UpdateTotalBalanceForMembers(dto.GroupId);
                if (updateTotalBalanceForMembers != null)
                {
                    DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                    return updateTotalBalanceForMembers;
                }
                
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetSettleDownTransaction",
                    new {id = settleDownTransaction.Id}, settleDownTransaction);
            }
            catch (Exception)
            {
                DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                throw;
            }
        }

        private async Task<ActionResult?> UpdateTotalBalanceForMembers(int? groupId)
        {
            if (_context.SettleDownMember == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownMember'  is null.");
            }
            if (_context.SettleDownDebt == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownDebt'  is null.");
            }
            
            List<SettleDownMember> members = await _context.SettleDownMember
                .Where(m => m.GroupId == groupId)
                .ToListAsync();
            foreach (SettleDownMember member in members)
            {
                decimal totalDebt = await _context.SettleDownDebt
                    .Where(d => d.MemberId == member.Id)
                    .Select(d => d.Amount)
                    .SumAsync();
                member.TotalBalance = -totalDebt;
            }
            return null;
        }

        private ActionResult<SettleDownTransaction>? AuthorizePostSettleDownTransaction(SettleDownTransactionDto dto)
        {
            if (_context.SettleDownTransaction == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownTransaction'  is null.");
            }

            if (!HasGroupClaimToTransaction(dto.GroupId))
            {
                return Unauthorized("You do not have access to this group!");
            }

            return null;
        }

        private async Task<ActionResult<SettleDownTransaction>?> PopulateDtoWithMemberGeneratedFromCurrentUser(
            SettleDownTransactionDto settleDownTransaction)
        {
            if (_context.SettleDownGroup == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownGroup'  is null.");
            }

            SettleDownGroup? group = await _context.SettleDownGroup.FindAsync(settleDownTransaction.GroupId);
            if (group == null)
            {
                return BadRequest("No such group exists!");
            }

            SettleDownMember? member = await InitializeMemberFromCurrentUserIfNotPresent(group);
            if (member == null)
            {
                await AddCurrentUserAsMemberForTransaction(settleDownTransaction);
                return null;
            }

            member = await _memberCrudService.Create(member);
            if (member == null)
                return Problem("Failed creation of 'SettleDownContext.SettleDownMember'!");
            
            return null;
        }

        private async Task AddCurrentUserAsMemberForTransaction(SettleDownTransactionDto settleDownTransaction)
        {
            string userNameClaim = User.Claims.First(c => c.Type == "userName").Value;
            settleDownTransaction.MemberId = (await _context.SettleDownMember!
                .Where(m => m.UserId == userNameClaim)
                .ToListAsync())[0].Id;
        }

        private async Task<SettleDownMember?> InitializeMemberFromCurrentUserIfNotPresent(SettleDownGroup settleDownGroup)
        {
            string userName = User.Claims.First(c => c.Type == "userName").Value;
            SettleDownUser currentUser = await GetCurrentUser(userName);

            if(await (_context.SettleDownMember ?? 
                      throw new InvalidOperationException("No value for 'SettleDownContext.SettleDownMember'!"))
               .AnyAsync(m => m.GroupId == settleDownGroup.Id && m.UserId == currentUser.UserName))
            {
                return null;
            }
            
            var member = new SettleDownMember
            {
                Name = userName,
                User = currentUser,
                GroupId = settleDownGroup.Id,
                Group = settleDownGroup,
                UserId = currentUser.UserName,
                TotalBalance = 0
            };
            return member;
        }

        private async Task<SettleDownUser> GetCurrentUser(string userName)
        {
            return await _context.SettleDownUser!.FindAsync(userName) 
                   ?? throw new InvalidOperationException("Data for current user not found!");
        }
        private SettleDownTransaction InitializeTransactionFromDto(SettleDownTransactionDto dto)
        {
            
            return new SettleDownTransaction
            {
                Name = dto.Name,
                Cost = dto.Cost,
                MemberId = dto.MemberId,
                GroupId = dto.GroupId
            };
        }

        // This hurts me deep inside. But it will be needed if Admin privileges get added.
        // // DELETE: api/Transaction/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteSettleDownTransaction(int id)
        // {
        //     
        //     if (_context.SettleDownTransaction == null)
        //     {
        //         return NotFound();
        //     }
        //     var settleDownTransaction = await _context.SettleDownTransaction.FindAsync(id);
        //     if (settleDownTransaction == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     _context.SettleDownTransaction.Remove(settleDownTransaction);
        //     await _context.SaveChangesAsync();
        //
        //     return NoContent();
        // }

        private bool SettleDownTransactionExists(int id)
        {
            return (_context.SettleDownTransaction?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}