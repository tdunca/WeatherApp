using Microsoft.EntityFrameworkCore;
using WeatherAppLab3.Models;

namespace WeatherAppLab3.DataAccess
{
    public class WeatherDBContext : DbContext
    {
        public DbSet<WeatherRecord> WeatherData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.db"); //add db here
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherRecord>().ToTable("Prod_WeatherData");
        }
    }
}