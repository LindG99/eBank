using eBank.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eBank.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<eBank.Models.BankAccount> BankAccounts { get; set; }
        public DbSet<eBank.Models.TransferHistory> TransferHistories { get; set; }
        public DbSet<eBank.Models.Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationships for TransferHistory
            modelBuilder.Entity<TransferHistory>()
                .HasOne(th => th.FromAccount)
                .WithMany()
                .HasForeignKey(th => th.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            // Prevent cascading deletes
            modelBuilder.Entity<TransferHistory>()
                .HasOne(th => th.ToAccount)
                .WithMany()
                .HasForeignKey(th => th.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
