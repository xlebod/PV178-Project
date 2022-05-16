using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SettleDown.Data;

namespace SettleDown.Helpers;

public static class DbContextRollbackHelper
{
    // Original code from:
    // Author: Jignesh Trivedi
    // Source: https://www.c-sharpcorner.com/UploadFile/ff2f08/discard-changes-without-disposing-dbcontextobjectcontext-in/
    // Code has not been modified and left in it's original form.
    public static void UndoChangesDbContextLevel(SettleDownContext context)  
    {  
        foreach (EntityEntry entry in context.ChangeTracker.Entries())  
        {  
            switch (entry.State)  
            {  
                case EntityState.Modified:  
                    entry.State = EntityState.Unchanged;  
                    break;  
                case EntityState.Added:  
                    entry.State = EntityState.Detached;  
                    break;    
                case EntityState.Deleted:  
                    entry.Reload();  
                    break;
                default: break;  
            }  
        }   
    }   
}