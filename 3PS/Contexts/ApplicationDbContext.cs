using _3PS.Models.Reporting;
using _3PS.Models.Vaccines;
using Microsoft.EntityFrameworkCore;

namespace _3PS.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
        }

        /* Data Models */
        public DbSet<VwVaccineBatchSend> VaccineBatchSends { get; set; }

        /* Reporting Models */
        public DbSet<VaccineReporting> VaccineReporting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* Data Models */
            modelBuilder.Entity<VwVaccineBatchSend>(entity =>
            {
                entity.ToTable("DBA - VaccineBatchSend");
                entity.HasNoKey();
            });

            /* Reporting Models */
            modelBuilder.Entity<VaccineReporting>(entity =>
            {
                entity.ToTable("DBA - VaccineBatchSendReport");
                entity.HasKey(x => x.RECNUM);
            });
        }
    }
}
