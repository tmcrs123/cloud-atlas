using System.Runtime.CompilerServices;
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

            builder.Entity<Atlas>()
            .HasMany(a => a.AtlasUsers)
            .WithOne()
            .HasForeignKey(au => au.AtlasId);

            // Marker
            builder.Entity<Marker>().Property(m => m.Title).HasMaxLength(30).IsRequired(required: true);

            builder.Entity<Marker>()
            .HasOne(m => m.PhotosLink)
            .WithOne()
            .HasForeignKey<MarkerPhotosLink>(pl => pl.MarkerId);

            //AtlasUsers
            builder.Entity<AtlasUser>().HasKey(au => new { au.AtlasId, au.UserId });

            builder.Entity<AtlasUser>()
            .HasOne(au => au.User)
            .WithMany(u => u.AtlasUsers)
            .HasForeignKey(au => au.UserId);

            builder.Entity<AtlasUser>()
            .HasOne(au => au.Atlas)
            .WithMany(u => u.AtlasUsers)
            .HasForeignKey(au => au.AtlasId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Atlas> Atlases { get; set; }
        public DbSet<AtlasUser> AtlasUsers { get; set; }
        public DbSet<Marker> Markers { get; set; }
        public DbSet<MarkerPhotosLink> PhotoLinks { get; set; }

    };

}
