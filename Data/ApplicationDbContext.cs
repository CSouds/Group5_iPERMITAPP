// ============================================================
// ApplicationDbContext - Database Context
// Configures Entity Framework Core with SQLite and defines
// all entity relationships for the iPERMIT system.
// Database name: Group5_iPERMITDB
// ============================================================

using Group5_iPERMITAPP.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5_iPERMITAPP.Data
{
    /// <summary>
    /// Entity Framework Core database context for Group5_iPERMITDB.
    /// Manages all entity sets and their relationships.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Entity sets (tables)
        public DbSet<RE> REs { get; set; }
        public DbSet<RESite> RESites { get; set; }
        public DbSet<EnvironmentalPermit> EnvironmentalPermits { get; set; }
        public DbSet<PermitRequest> PermitRequests { get; set; }
        public DbSet<EO> EOs { get; set; }
        public DbSet<Decision> Decisions { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Permit> Permits { get; set; }
        public DbSet<EmailArchive> EmailArchives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RE -> RESite (1 to many, composition)
            modelBuilder.Entity<RESite>()
                .HasOne(s => s.RE)
                .WithMany(r => r.Sites)
                .HasForeignKey(s => s.REID)
                .OnDelete(DeleteBehavior.Cascade);

            // RE -> PermitRequest (1 to many)
            modelBuilder.Entity<PermitRequest>()
                .HasOne(pr => pr.RequestedBy)
                .WithMany(r => r.PermitRequests)
                .HasForeignKey(pr => pr.REID)
                .OnDelete(DeleteBehavior.Cascade);

            // PermitRequest -> EnvironmentalPermit (many to 1)
            modelBuilder.Entity<PermitRequest>()
                .HasOne(pr => pr.RequestedPermit)
                .WithMany()
                .HasForeignKey(pr => pr.EnvironmentalPermitID)
                .OnDelete(DeleteBehavior.Restrict);

            // PermitRequest -> RequestStatus (1 to many)
            modelBuilder.Entity<RequestStatus>()
                .HasOne(rs => rs.PermitRequest)
                .WithMany(pr => pr.Statuses)
                .HasForeignKey(rs => rs.PermitRequestNo)
                .OnDelete(DeleteBehavior.Cascade);

            // PermitRequest -> Payment (1 to 0..1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PermitRequest)
                .WithOne(pr => pr.Payment)
                .HasForeignKey<Payment>(p => p.PermitRequestNo)
                .OnDelete(DeleteBehavior.Cascade);

            // PermitRequest -> Decision (1 to 0..1)
            modelBuilder.Entity<Decision>()
                .HasOne(d => d.RelatedTo)
                .WithOne(pr => pr.Decision)
                .HasForeignKey<Decision>(d => d.PermitRequestNo)
                .OnDelete(DeleteBehavior.Cascade);

            // EO -> Decision (1 to many)
            modelBuilder.Entity<Decision>()
                .HasOne(d => d.MadeBy)
                .WithMany()
                .HasForeignKey(d => d.EOID)
                .OnDelete(DeleteBehavior.Restrict);

            // PermitRequest -> Permit (1 to 0..1)
            modelBuilder.Entity<Permit>()
                .HasOne(p => p.RelatedTo)
                .WithOne(pr => pr.IssuedPermit)
                .HasForeignKey<Permit>(p => p.PermitRequestNo)
                .OnDelete(DeleteBehavior.Cascade);

            // EO -> Permit (1 to many)
            modelBuilder.Entity<Permit>()
                .HasOne(p => p.IssuedBy)
                .WithMany()
                .HasForeignKey(p => p.EOID)
                .OnDelete(DeleteBehavior.Restrict);

            // RE -> Permit (1 to many)
            modelBuilder.Entity<Permit>()
                .HasOne(p => p.IssuedTo)
                .WithMany()
                .HasForeignKey(p => p.REID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
