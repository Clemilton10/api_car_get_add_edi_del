using Microsoft.EntityFrameworkCore;
using MyCar.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyCar.Context;

public class AppDbContext : DbContext
{
	public DbSet<Car> Cars { get; set; }
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		IConfiguration configuration = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", true)
		.Build();
		optionsBuilder.UseSqlite(configuration.GetConnectionString("ServerConnection"));
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Car>()
			.ToTable("Car");

		modelBuilder.Entity<Car>()
			.HasKey(c => c.Id);

		modelBuilder.Entity<Car>()
			.Property(c => c.Plate)
			.HasColumnType("TEXT")
			.IsRequired()
			.HasDefaultValue("");

		modelBuilder.Entity<Car>()
			.Property(c => c.Brand)
			.HasColumnType("TEXT")
			.IsRequired()
			.HasDefaultValue("");

		modelBuilder.Entity<Car>()
			.Property(c => c.Price)
			.HasColumnType("REAL")
			.IsRequired()
			.HasDefaultValue(0.0m);
	}
}