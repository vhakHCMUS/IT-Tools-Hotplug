﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TKPM_Project.Models;
using TKPM_Project.Models.Tools;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Tool> Tools { get; set; }
    public DbSet<UserLikedTool> UserLikedTools { get; set; }
    public DbSet<UserPremium> UserPremiums { get; set; }


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

        modelBuilder.Entity<UserLikedTool>()
        .HasKey(ult => new { ult.UserId, ult.ToolId });

        modelBuilder.Entity<UserLikedTool>()
        .HasOne(ult => ult.User)
        .WithMany()
        .HasForeignKey(ult => ult.UserId);

        modelBuilder.Entity<UserLikedTool>()
            .HasOne(ult => ult.Tool)
            .WithMany()
            .HasForeignKey(ult => ult.ToolId);

        modelBuilder.Entity<UserPremium>()
    .HasKey(up => up.Id);

        modelBuilder.Entity<UserPremium>()
            .HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPremium>()
            .HasIndex(up => up.ExpireDate); // Index để dễ truy vấn expire


        modelBuilder.Entity<IdentityRole>().HasData(roles);
    }
}
