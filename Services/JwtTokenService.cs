using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Models;

namespace SettleDown.Services;

public class SettleJwtService : ITokenService
{
    private const string JwtMemberClaimDelimiter = ";";
    private const string JwtGroupClaimDelimiter = ";";

    private readonly SettleDownContext _context;
    private readonly IConfiguration _config;

    public SettleJwtService(SettleDownContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<string> GenerateToken(SettleDownUserDto user)
    {
        string key = Environment.GetEnvironmentVariable("JwtIssuerKey") ?? 
                     throw new KeyNotFoundException("No 'JwtIssuerKey' found in env. variables!");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var permClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("valid", "1"),
            new ("userName", user.UserName),
            await GetGroupClaimsForUser(user.UserName),
            await GetMemberClaimsForUser(user.UserName)
        };

        var token = new JwtSecurityToken(_config["JwtIssuer"],
            _config["JwtAudience"],
            permClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<Claim> GetMemberClaimsForUser(string userName)
    {
        const string claimKey = "members";

        var claimValue = "";
        if (_context.SettleDownUser == null)
        {
            return new Claim(claimKey, claimValue);
        }

        IEnumerable<int> memberIds = await (
            from m in _context.SettleDownMember
            where m.UserId == userName
            select m.Id).ToListAsync();
        
        return GenerateClaim(memberIds, claimKey);
    }

    private static Claim GenerateClaim(IEnumerable<int> ids, string claimKey)
    {
        var claimValue = ids.Aggregate(new StringBuilder(), (s, i) =>
                s.Append(JwtMemberClaimDelimiter + i))
            .ToString();

        if (!claimValue.IsNullOrEmpty())
        {
            claimValue = claimValue.Remove(0, 1);
        }

        return new Claim(claimKey, claimValue);
    }

    private async Task<Claim> GetGroupClaimsForUser(string userName)
    {
        const string claimKey = "groups";
        var claimValue = "";
        if (_context.SettleDownUser == null)
        {
            return new Claim(claimKey, claimValue);
        }
        
        IEnumerable<int> groupIds = await (
            from g in _context.SettleDownGroup
            join m in _context.SettleDownMember on g.Id equals m.GroupId
            where m.UserId == userName
            select g.Id).ToListAsync();

        return GenerateClaim(groupIds, claimKey);
    }
}