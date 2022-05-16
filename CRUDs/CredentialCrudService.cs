using Microsoft.EntityFrameworkCore;
using SettleDown.Data;
using SettleDown.Models;
// ReSharper disable InconsistentNaming

namespace SettleDown.CRUDs;

public class CredentialCrudService : ICrudService<SettleDownCredential>
{
    private readonly SettleDownContext _context;

    public CredentialCrudService(SettleDownContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Update(string id, SettleDownCredential settleDownCredential)
    {
        return await Update(id, settleDownCredential, true);
    }

    private async Task<bool> Update(string id, SettleDownCredential settleDownCredential, bool saveChanges)
    {
        if (int.Parse(id) != settleDownCredential.Id)
        {
            return false;
        }
        if (!SettleDownCredentialExists(int.Parse(id)))
        {
            return false;
        }

        _context.Entry(settleDownCredential).State = EntityState.Modified;

        if (!saveChanges)
            await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SettleDownCredential?> Create(SettleDownCredential settleDownCredential)
    {
        return await Create(settleDownCredential, true);
    }

    private async Task<SettleDownCredential?> Create(SettleDownCredential settleDownCredential, bool saveChanges)
    {
        if (_context.SettleDownCredential == null)
        {
            return null;
        }
        
        if (SettleDownCredentialExists(settleDownCredential.Id))
        {
            return null;
        }


        _context.SettleDownCredential.Add(settleDownCredential);
        if (saveChanges)
            await _context.SaveChangesAsync();

        return settleDownCredential;
    }


    public async Task Delete(string id)
    {
        await Delete(id, true);
    }

    private async Task Delete(string id, bool saveChanges)
    {
        if (_context.SettleDownCredential == null)
        {
            return;
        }

        var settleDownCredential = await _context.SettleDownCredential.FindAsync(id);
        if (settleDownCredential == null)
        {
            return;
        }

        _context.SettleDownCredential.Remove(settleDownCredential);
        if(saveChanges)
            await _context.SaveChangesAsync();
    }

    public async Task<SettleDownCredential?> CreateWithoutSaving(SettleDownCredential settleDownCredential)
    {
        return await Create(settleDownCredential, false);
    }

    public async Task<bool> UpdateWithoutSaving(string id, SettleDownCredential settleDownCredential)
    {
        return await Update(id, settleDownCredential, false);
    }

    public async Task DeleteWithoutSaving(string id)
    {
        await Delete(id, false);
    }

    private bool SettleDownCredentialExists(int id)
    {
        return (_context.SettleDownCredential?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}