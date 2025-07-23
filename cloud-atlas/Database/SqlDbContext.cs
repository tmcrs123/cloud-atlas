using System.Runtime.CompilerServices;
using cloud_atlas.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace cloud_atlas
{
    public class SqlDbContext : DbContext
    {
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        public SqlDbContext(DbContextOptions<SqlDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            HttpContextAccessor = httpContextAccessor;
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

            //AtlasUsers
            builder.Entity<AtlasUser>().HasKey(au => new { au.AtlasId, au.UserId });

            builder.Entity<AtlasUser>()
            .HasOne(au => au.Atlas)
            .WithMany(u => u.AtlasUsers)
            .HasForeignKey(au => au.AtlasId);
        }

        public DbSet<Atlas> Atlases { get; set; }
        public DbSet<AtlasUser> AtlasUsers { get; set; }
        public DbSet<Marker> Markers { get; set; }

        // helpers

        public async Task<bool> UserOwnsMap(Guid atlasId)
        {
            var sub = HttpContextAccessor.HttpContext.Items["sub"] as string;

            if (string.IsNullOrEmpty(sub))
            {
                return false;
            }

            var atlasUser = await AtlasUsers
            .FirstOrDefaultAsync(au => au.AtlasId == atlasId && au.UserId == new Guid(sub));

            if (atlasUser?.IsOwner == false) return false;
            return true;
        }
    };

}
