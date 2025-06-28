using LibManage.Data.Models.Library;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Prevents the user from writing multiple reviews on a same book
        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.BookId })
            .IsUnique();
    }
}
