using cloud_atlas.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace cloud_atlas
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Atlas
            builder.Entity<Atlas>().Property(a => a.Title).HasMaxLength(30).IsRequired(required: true);

            // Marker
            builder.Entity<Marker>().Property(m => m.Title).HasMaxLength(30).IsRequired(required: true);

            //AtlasUsers
            builder.Entity<AtlasUsers>().HasKey(au => new { au.AtlasId, au.UserId });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Atlas> Atlases { get; set; }
        public DbSet<AtlasUsers> AtlasUsers { get; set; }
        public DbSet<Marker> Markers { get; set; }
        public DbSet<MarkerPhotosLink> PhotoDetailsLinks { get; set; }

    };

}
