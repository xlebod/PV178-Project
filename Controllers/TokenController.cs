using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Helpers;
using SettleDown.Models;
using SettleDown.Services;

namespace SettleDown.Controllers;

[Route("api/[controller]")]
[ApiController]
// ReSharper disable once InconsistentNaming
public class TokenController : ControllerBase
{
    private readonly SettleDownContext _context;
    private readonly ITokenService _tokenService;

    public TokenController(SettleDownContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    
    // POST: api/Login
    [HttpPost]
    public async Task<ActionResult<string>> Login(SettleDownUserDto userDto)
    {
        if (_context.SettleDownUser == null)
        {
            return Problem("Entity set 'SettleDownContext.SettleDownUser'  is null.");
        }

        SettleDownUser? user =  await _context.SettleDownUser
            .Where(u => u.UserName == userDto.UserName)
            .Include(u => u.Credentials)
            .FirstOrDefaultAsync();
            
        if (user == null)
            return BadRequest("Provided user-password combination is invalid!");

        if (!AuthorizationHandler.AreCredentialsValid(user.Credentials, userDto.Password))
            return BadRequest("Provided user-password combination is invalid!");

        return await _tokenService.GenerateToken(userDto);
    }
}