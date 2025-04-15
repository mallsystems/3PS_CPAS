using _3PS.Models.Vaccines;
using Microsoft.EntityFrameworkCore;

namespace _3PS.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
        }

        public DbSet<VwVaccineBatchSend> VaccineBatchSends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VwVaccineBatchSend>(entity =>
            {
                entity.ToView("vw_VaccineBatchSend");
                entity.HasNoKey();
            });
        }
    }
}
