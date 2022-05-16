using Microsoft.EntityFrameworkCore;

namespace SettleDown.Data
{
    public class SettleDownContext : DbContext
    {
        public SettleDownContext (DbContextOptions<SettleDownContext> options)
            : base(options)
        {
        }

        public DbSet<Models.SettleDownUser>? SettleDownUser { get; set; }

        public DbSet<Models.SettleDownCredential>? SettleDownCredential { get; set; }

        public DbSet<Models.SettleDownGroup>? SettleDownGroup { get; set; }
        
        public DbSet<Models.SettleDownTransaction>? SettleDownTransaction { get; set; }
        
        public DbSet<Models.SettleDownDebt>? SettleDownDebt { get; set; }
        
        public DbSet<Models.SettleDownMember>? SettleDownMember { get; set; }
    }
}
