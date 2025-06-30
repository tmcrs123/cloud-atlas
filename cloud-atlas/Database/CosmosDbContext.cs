using cloud_atlas.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace cloud_atlas
{
    public class CosmosDbContext : DbContext
    {
        public CosmosDbContext(DbContextOptions<CosmosDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultContainer("MarkerPhotos");
            builder.Entity<MarkerPhotos>().HasPartitionKey(mp => mp.MarkerId);
        }



        public DbSet<MarkerPhotos> MarkerPhotos { get; set; }

    };

}
