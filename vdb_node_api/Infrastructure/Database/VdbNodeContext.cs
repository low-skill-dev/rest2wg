using Microsoft.EntityFrameworkCore;
using vdb_node_api.Models.Database;

namespace vdb_node_api.Infrastructure.Database
{
	public class VdbNodeContext : DbContext
	{
		public DbSet<ApplicationAccount> ApplicationAccounts { get; set; } = null!;

		public VdbNodeContext()
			: base()
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}

		
	}
}
