using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Models;

namespace SettleDown.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly SettleDownContext _context;
        private const string JwtMemberClaimDelimiter = ";";
        public MemberController(SettleDownContext context)
        {
            _context = context;
        }

        // GET: api/Member
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettleDownMember>>> GetSettleDownMember()
        {
          if (_context.SettleDownMember == null)
          {
              return NotFound();
          }
          
          string[] memberClaims = User.Claims.First(c => c.Type == "members").Value
              .Split(JwtMemberClaimDelimiter);

          if (memberClaims[0].IsNullOrEmpty())
              return new List<SettleDownMember>();

          List<SettleDownMember> allMembers = new ();

          foreach (string claim in memberClaims)
          {
              SettleDownMember? member = await GetMemberFromClaim(claim);
              if (member == null)
                  continue;
              allMembers.Add(member);
          }
          return allMembers;
        }
        
        private async Task<SettleDownMember?> GetMemberFromClaim(string claim)
        {
            if(int.TryParse(claim, out int id))
                return await _context.SettleDownMember!.FindAsync(id);
            return null;
        }
        
        // GET: api/Member/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SettleDownMember>> GetSettleDownMember(int id)
        {
            if (_context.SettleDownMember == null)
            {
              return NotFound();
            }
            
            SettleDownMember? settleDownMember = await _context.SettleDownMember.FindAsync(id);
            
            if (settleDownMember == null)
            {
                return NotFound();
            }
            
            if (!HasMemberClaim(id))
            {
                return Unauthorized("You don't have access to that member!");
            }
            return settleDownMember;
        }

        // PUT: api/Member/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSettleDownMember(int id, SettleDownMemberPutDto settleDownMemberDto)
        {
            IActionResult? authorizeAndValidatePutRequest = await AuthorizeAndValidatePutRequest(id);
            if(authorizeAndValidatePutRequest != null)
            {
                return authorizeAndValidatePutRequest;
            }

            SettleDownMember originalMember = (await _context.SettleDownMember!.FindAsync(id))!;
            originalMember.Name = settleDownMemberDto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SettleDownMemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private async Task<IActionResult?> AuthorizeAndValidatePutRequest(int id)
        {
            if (_context.SettleDownMember == null)
            {
                return Problem("Entity set 'SettleDownContext.SettleDownMember'  is null.");
            }

            SettleDownMember? originalMember = await _context.SettleDownMember.FindAsync(id);
            
            if (originalMember == null)
            {
                return NotFound("Member with such an Id was not found!");
            }

            if (!HasMemberClaim(id))
            {
                return Unauthorized("You don't have access to that member!");
            }

            return null;
        }

        // POST: api/Member
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SettleDownMember>> PostSettleDownMember(SettleDownMemberDto dto)
        {
            if (_context.SettleDownMember == null)
            {
              return Problem("Entity set 'SettleDownContext.SettleDownMember'  is null.");
            }

            if (!HasGroupClaim(dto.GroupId))
            {
                return Unauthorized("You cannot add members to that group!");
            }

            var settleDownMember = new SettleDownMember
            {
                Name = dto.Name,
                GroupId = dto.GroupId,
                TotalBalance = 0,
                UserId = dto.UserId
            };
            
            _context.SettleDownMember.Add(settleDownMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSettleDownMember", new { id = settleDownMember.Id }, settleDownMember);
        }

        private bool HasMemberClaim(int? id)
        {
            if (id == null)
                return false;
            
            string[] memberClaims = User.Claims.First(c => c.Type == "members").Value
                .Split(JwtMemberClaimDelimiter);

            return memberClaims.Contains(id.ToString());
        }
        
        private bool HasGroupClaim(int? id)
        {
            if (id == null)
                return false;
            
            string[] groupClaims = User.Claims.First(c => c.Type == "groups").Value
                .Split(JwtMemberClaimDelimiter);

            return groupClaims.Contains(id.ToString());
        }

        private bool SettleDownMemberExists(int id)
        {
            return (_context.SettleDownMember?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
