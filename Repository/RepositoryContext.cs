using Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Repository.Configuration;

namespace Repository
{
	public class RepositoryContext:IdentityDbContext<User>
	{

		public RepositoryContext(DbContextOptions options) : base( options ) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//Additionally, we call the OnModelCreating method from the base class. This is required for migration to work properly
			
						base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfiguration( new CompanyConfiguration() );
			modelBuilder.ApplyConfiguration(new EmployeeConfiguration() );
			modelBuilder.ApplyConfiguration(new RoleConfiguration() );
		}
		public DbSet<Company> Companies { get; set; }
		public DbSet<Employee> Employees { get; set; }




	}
}