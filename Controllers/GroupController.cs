using Castle.Components.DictionaryAdapter;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettleDown.CRUDs;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Helpers;
using SettleDown.Models;

namespace SettleDown.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // ReSharper disable once InconsistentNaming
    public class GroupController : ControllerBase
    {
        private const char JwtGroupClaimDelimiter = ';';

        private readonly SettleDownContext _context;
        private readonly ICrudService<SettleDownMember> _memberCrudService;
        
        public GroupController(SettleDownContext context, ICrudService<SettleDownMember> memberCrudService)
        {
            _context = context;
            _memberCrudService = memberCrudService;
        }

        // GET: api/Group
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettleDownGroup>>> GetSettleDownGroup()
        {
            if (_context.SettleDownGroup == null)
            {
              return NotFound();
            }

            string[] groupClaims = User.Claims.First(c => c.Type == "groups").Value
              .Split(JwtGroupClaimDelimiter);

            if (groupClaims[0].IsNullOrEmpty())
                return new List<SettleDownGroup>();
            
            IEnumerable<SettleDownGroup> groups = new List<SettleDownGroup>();
            foreach (string claim in groupClaims)
            {
              int id = int.Parse(claim);
              SettleDownGroup? group = await _context.SettleDownGroup.FindAsync(id);
              if (group == null)
                  continue;    
              groups = groups.Append(group);
            }
            return new ActionResult<IEnumerable<SettleDownGroup>>(groups);
        }

        // GET: api/Group/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SettleDownGroupDto>> GetSettleDownGroup(int id)
        {
            if (_context.SettleDownGroup == null)
            {
              return NotFound();
            }

            if (!HasGroupClaim(id))
                return Unauthorized("You don't have access to that group!");

            SettleDownGroup? group = await _context.SettleDownGroup.FindAsync(id);
            if (group == null)
                return NotFound("No such group exists!");

            ActionResult<List<SettleDownMember>> members = await GetMembersForGroup(id);
            if (members.Result != null)
            {
                return members.Result;
            }
            ActionResult<List<SettleDownDebt>> debts = await GetDebtsForGroup(id);
            if (debts.Result != null)
            {
                return debts.Result;
            }
            
            return new SettleDownGroupDto
            {
                Name = group.Name,
                Debts = debts.Value ?? new List<SettleDownDebt>(),
                Members = members.Value ?? new List<SettleDownMember>()
            };
        }

        private async Task<ActionResult<List<SettleDownDebt>>> GetDebtsForGroup(int id)
        {
            if (_context.SettleDownDebt == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownDebt'  is null.");
            }
            return await _context.SettleDownDebt.Where(d => d.GroupId == id).ToListAsync();
        }

        private async Task<ActionResult<List<SettleDownMember>>> GetMembersForGroup(int id)
        {
            if (_context.SettleDownMember == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownMember'  is null.");
            }
            return await _context.SettleDownMember.Where(m => m.GroupId == id).ToListAsync();
        }

        private bool HasGroupClaim(int id)
        {
            string[] groupClaims = User.Claims.First(c => c.Type == "groups").Value
                .Split(JwtGroupClaimDelimiter);
            return groupClaims.Contains(id.ToString());
        }

        // PUT: api/Group/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSettleDownGroup(int id, SettleDownGroup settleDownGroup)
        {
            if (id != settleDownGroup.Id)
            {
                return BadRequest();
            }
            if (!HasGroupClaim(id))
            {
                return Unauthorized("You don't have access to that group!");
            }

            _context.Entry(settleDownGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SettleDownGroupExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/Group
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SettleDownGroup>> PostSettleDownGroup(SettleDownGroupDto dto)
        {
            if (_context.SettleDownGroup == null)
            {
              return Problem("Entity set 'SettleDownContext.SettleDownGroup'  is null.");
            }
            if (_context.SettleDownGroup == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownUser'  is null.");
            }

            SettleDownGroup settleDownGroup = InitializeGroupFromDto(dto);
            _context.SettleDownGroup.Add(settleDownGroup);

            SettleDownMember? memberFromCurrentUser = await InitializeMemberFromCurrentUserIfNotPresent(settleDownGroup);
            
            if (memberFromCurrentUser == null)
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetSettleDownGroup", new { id = settleDownGroup.Id },
                    settleDownGroup);
            }

            memberFromCurrentUser = await _memberCrudService.CreateWithoutSaving(memberFromCurrentUser);
            if (memberFromCurrentUser == null)
            {
                DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
                return Problem("Could not create entity 'SettleDownContext.SettleDownUser'");
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction("GetSettleDownGroup", new {id = settleDownGroup.Id}, settleDownGroup);
        }

        private static SettleDownGroup InitializeGroupFromDto(SettleDownGroupDto dto)
        {
            var group = new SettleDownGroup
            {
                Name = dto.Name
            };
            return group;
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

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSettleDownGroup(int id)
        {
            if (_context.SettleDownGroup == null)
            {
                return NotFound();
            }
            var settleDownGroup = await _context.SettleDownGroup.FindAsync(id);
            if (settleDownGroup == null)
            {
                return NotFound();
            }

            _context.SettleDownGroup.Remove(settleDownGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ReSharper disable once InconsistentNaming
        private bool SettleDownGroupExists(int id)
        {
            return (_context.SettleDownGroup?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
