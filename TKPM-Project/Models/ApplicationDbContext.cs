using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TKPM_Project.Models.Tools;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Tool> Tools { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var roles = new List<IdentityRole>
    {
        new IdentityRole
        {
            Id = "8a16a1c6-77b2-4a7e-b5e1-8b9c2e8df845",
            Name = "Anonymous",
            NormalizedName = "ANONYMOUS"
        },
        new IdentityRole
        {
            Id = "a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d", // GUID cố định
            Name = "User",
            NormalizedName = "USER"
        },
        new IdentityRole
        {
            Id = "9d8c7b6a-5e4f-3d2c-1b0a-9f8e7d6c5b4a",
            Name = "Premium",
            NormalizedName = "PREMIUM"
        },
        new IdentityRole
        {
            Id = "b3a2c1d0-e9f8-7d6c-5b4a-3e2f1c0d9a8b",
            Name = "Admin",
            NormalizedName = "ADMIN"
        }
    };


        modelBuilder.Entity<IdentityRole>().HasData(roles);
    }
}
