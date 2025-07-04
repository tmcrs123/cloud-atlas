using cloud_atlas.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace cloud_atlas
{
    public class CosmosDbContext : DbContext
    {
        public IConfiguration Configuration { get; set; }
        public CosmosDbContext(DbContextOptions<CosmosDbContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultContainer(Configuration.GetValue<string>("CosmosDb:ContainerName"));
            builder.Entity<MarkerPhotos>().HasKey(mp => mp.PhotoLinkId);
            builder.Entity<MarkerPhotos>().HasPartitionKey(mp => mp.MarkerId);
        }

        public DbSet<MarkerPhotos> MarkerPhotos { get; set; }
    };
}
