// Services/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using System;
using WheelTracker.Models;

namespace WheelTracker.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<Trade> Trades { get; set; } = null!;

        private readonly string _dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WheelTracker.db");

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");

        // THIS CREATES THE TABLE IF IT DOESN'T EXIST
        public void EnsureDatabaseCreated()
        {
            Database.Migrate();  // Applies migrations OR creates DB if none exist
        }
    }
}