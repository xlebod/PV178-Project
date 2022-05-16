using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.Models;

namespace SettleDown.CRUDs;

public class MemberCrudService : ICrudService<SettleDownMember>
{
    private readonly SettleDownContext _context;

    public MemberCrudService(SettleDownContext context)
    {
        _context = context;
    }
        
    public async Task<bool> Update(string id, SettleDownMember settleDownMember)
    {
        return await Update(id, settleDownMember, true);
    }

    private async Task<bool> Update(string id, SettleDownMember settleDownMember, bool saveChanges)
    {
        if (int.Parse(id) != settleDownMember.Id)
        {
            return false;
        }
        if (!SettleDownMemberExists(int.Parse(id)))
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

    public async Task<SettleDownMember?> Create(SettleDownMember settleDownMember)
    {
        return await Create(settleDownMember, true);
    }

    private async Task<SettleDownMember?> Create(SettleDownMember settleDownMember, bool saveChanges)
    {
        if (_context.SettleDownMember == null)
        {
            return null;
        }
        if (SettleDownMemberExists(settleDownMember.Id))
        {
            return null;
        }

        _context.SettleDownMember.Add(settleDownMember); 
        if(saveChanges)
            await _context.SaveChangesAsync();
        
        return settleDownMember;
    }

    public async Task Delete(string id)
    {
        await Delete(id, true);
    }

    private async Task Delete(string id, bool saveChanges)
    {
        if (_context.SettleDownMember == null)
        {
            return;
        }

        SettleDownMember? settleDownMember = await _context.SettleDownMember.FindAsync(id);
        if (settleDownMember == null)
        {
            return;
        }

        _context.SettleDownMember.Remove(settleDownMember);
        if(saveChanges)
            await _context.SaveChangesAsync();
    }

    public async Task<SettleDownMember?> CreateWithoutSaving(SettleDownMember settleDownMember)
    {
        return await Create(settleDownMember, false);
    }

    public async Task<bool> UpdateWithoutSaving(string id, SettleDownMember settleDownMember)
    {
        return await Update(id, settleDownMember, false);
    }

    public async Task DeleteWithoutSaving(string id)
    {
        await Delete(id, false);
    }

    private bool SettleDownMemberExists(int id)
    {
        return (_context.SettleDownMember?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}