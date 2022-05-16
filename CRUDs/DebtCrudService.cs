using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.Models;

namespace SettleDown.CRUDs;

public class DebtCrudService : ICrudService<SettleDownDebt>
{
    private readonly SettleDownContext _context;

    public DebtCrudService(SettleDownContext context)
    {
        _context = context;
    }
        
    public async Task<bool> Update(string id, SettleDownDebt settleDownDebt)
    {
        return await Update(id, settleDownDebt, true);
    }

    private async Task<bool> Update(string id, SettleDownDebt settleDownDebt, bool saveChanges)
    {
        if (int.Parse(id) != settleDownDebt.Id)
        {
            return false;
        }

        if (!SettleDownDebtExists(int.Parse(id)))
        {
            return false;
        }
        
        _context.Entry(settleDownDebt).State = EntityState.Modified;

        if (saveChanges)
            await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SettleDownDebt?> Create(SettleDownDebt settleDownDebt)
    {
        return await Create(settleDownDebt, true);
    }

    private async Task<SettleDownDebt?> Create(SettleDownDebt settleDownDebt, bool saveChanges)
    {
        if (_context.SettleDownDebt == null)
        {
            return null;
        }
        if (SettleDownDebtExists(settleDownDebt.Id))
        {
            return null;
        }

        _context.SettleDownDebt.Add(settleDownDebt);
        
        if (saveChanges)
            await _context.SaveChangesAsync();

        return settleDownDebt;
    }

    public async Task Delete(string id)
    {
        await Delete(id, true);
    }

    private async Task Delete(string id, bool saveChanges)
    {
        if (_context.SettleDownDebt == null)
        {
            return;
        }

        SettleDownDebt? settleDownDebt = await _context.SettleDownDebt.FindAsync(id);
        if (settleDownDebt == null)
        {
            return;
        }

        _context.SettleDownDebt.Remove(settleDownDebt);
        if(saveChanges)
            await _context.SaveChangesAsync();
    }

    public async Task<SettleDownDebt?> CreateWithoutSaving(SettleDownDebt settleDownMember)
    {
        return await Create(settleDownMember, false);
    }

    public async Task<bool> UpdateWithoutSaving(string id, SettleDownDebt settleDownMember)
    {
        return await Update(id, settleDownMember, false);
    }

    public async Task DeleteWithoutSaving(string id)
    {
        await Delete(id, false);
    }

    private bool SettleDownDebtExists(int id)
    {
        return (_context.SettleDownDebt?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}