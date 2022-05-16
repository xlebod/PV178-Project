using Microsoft.AspNetCore.Mvc;
using SettleDown.CRUDs;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Helpers;
using SettleDown.Models;

namespace SettleDown.Controllers;

[Route("api/[controller]")]
[ApiController]
// ReSharper disable once InconsistentNaming
public class RegisterController : ControllerBase
{
    private readonly SettleDownContext _context;
    private readonly ICrudService<SettleDownUser> _userCrudService;
    private readonly ICrudService<SettleDownCredential> _credentialCrudService;

    public RegisterController(SettleDownContext context,
        ICrudService<SettleDownUser> userCrudService,
        ICrudService<SettleDownCredential> credentialCrudService)
    {
        _context = context;
        _userCrudService = userCrudService;
        _credentialCrudService = credentialCrudService;
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> RegisterNewUser(SettleDownUserDto userDto)
    {
        if (_context.SettleDownUser == null)
        {
            return Problem("Entity set 'SettleDownContext.SettleDownUser'  is null.");
        }
        
        if (_context.SettleDownCredential == null)
        {
            return Problem("Entity set 'SettleDownContext.SettleDownCredential'  is null.");
        }

        if (SettleDownUserExists(userDto.UserName))
            return Conflict("User with such a name already exists!");
        
        SettleDownCredential? credentials = InitializeCredentialsWithData(userDto);
        credentials = await _credentialCrudService.CreateWithoutSaving(credentials);

        if (credentials == null)
        {
            DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
            return Problem("Oh no! Something went wrong when generating 'SettleDownCredentials'!");
        }        
        
        SettleDownUser? user = InitializeUserWithData(userDto, credentials);
        user = await _userCrudService.CreateWithoutSaving(user);

        if (user == null)
        {
            DbContextRollbackHelper.UndoChangesDbContextLevel(_context);
            return Problem("Oh no! Something went wrong when generating 'SettleDownUser'!");
        }

        await _context.SaveChangesAsync();
        return CreatedAtAction("RegisterNewUser", new {id = user.UserName}, user);
    }

    private static SettleDownUser InitializeUserWithData(SettleDownUserDto userDto, SettleDownCredential credentials)
    {
        SettleDownUser user = new()
        {
            UserName = userDto.UserName,
            Credentials = credentials
        };
        return user;
    }

    private static SettleDownCredential InitializeCredentialsWithData(SettleDownUserDto userDto)
    {
        byte[] salt = AuthorizationHandler.Create128BitSalt();
        string password = AuthorizationHandler.CreateSaltedHashPassword(userDto.Password, salt);
        SettleDownCredential credentials = new()
        {
            Password = password,
            Salt = salt
        };
        return credentials;
    }

    // ReSharper disable once InconsistentNaming
    private bool SettleDownUserExists(string userName)
    {
        return (_context.SettleDownUser?.Any(e => e.UserName == userName)).GetValueOrDefault();
    }

}