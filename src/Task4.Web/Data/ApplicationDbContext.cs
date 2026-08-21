using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task4.Web.Models;

namespace Task4.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.RegistrationTime)
                .IsRequired();
        });

        builder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.OccurredAt
            });

            entity.HasOne(x => x.User)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.OccurredAt)
                .IsRequired();

            entity.Property(x => x.ActivityType)
                .IsRequired();
        });
    }
}