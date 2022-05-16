using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.Models;

namespace SettleDown.CRUDs;

public class UserCrudService : ICrudService<SettleDownUser>
{
    private readonly SettleDownContext _context;

    public UserCrudService(SettleDownContext context)
    {
        _context = context;
    }
        
    public async Task<bool> Update(string id, SettleDownUser settleDownMember)
    {
        return await Update(id, settleDownMember, true);
    }

    private async Task<bool> Update(string id, SettleDownUser settleDownMember, bool saveChanges)
    {
        if (id != settleDownMember.UserName)
        {
            return false;
        }
        if (!SettleDownUserExists(id))
        {
            return false;
        }

        _context.Entry(settleDownMember).State = EntityState.Modified;
        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<SettleDownUser?> Create(SettleDownUser settleDownMember)
    {
        return await Create(settleDownMember, true);
    }

    private async Task<SettleDownUser?> Create(SettleDownUser settleDownMember, bool saveChanges)
    {
        if (_context.SettleDownUser == null)
        {
            return null;
        }
        if (SettleDownUserExists(settleDownMember.UserName))
        {
            return null;
        }

        _context.SettleDownUser.Add(settleDownMember);
        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return settleDownMember;
    }

    public async Task Delete(string id)
    {
        await Delete(id, true);
    }

    private async Task Delete(string id, bool saveChanges)
    {
        if (_context.SettleDownUser == null)
        {
            return;
        }

        SettleDownUser? settleDownUser = await _context.SettleDownUser.FindAsync(id);
        if (settleDownUser == null)
        {
            return;
        }

        _context.SettleDownUser.Remove(settleDownUser);
        if(saveChanges)
            await _context.SaveChangesAsync();
    }

    public async Task<SettleDownUser?> CreateWithoutSaving(SettleDownUser settleDownMember)
    {
        return await Create(settleDownMember, false);
    }

    public async Task<bool> UpdateWithoutSaving(string id, SettleDownUser settleDownMember)
    {
        return await Update(id, settleDownMember, false);
    }

    public async Task DeleteWithoutSaving(string id)
    {
        await Delete(id, false);
    }

    private bool SettleDownUserExists(string id)
    {
        return (_context.SettleDownUser?.Any(e => e.UserName == id)).GetValueOrDefault();
    }
}