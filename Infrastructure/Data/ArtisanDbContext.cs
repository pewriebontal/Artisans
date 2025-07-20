using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Artisans.Models;

namespace Artisans.Infrastructure.Data
{
    
    public class ArtisansDBContext : IdentityDbContext<User, Role, int,
                                       IdentityUserClaim<int>, IdentityUserRole<int>,
                                       IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ArtisansDBContext(DbContextOptions<ArtisansDBContext> options) : base(options)
        {
        }

        
        

        public DbSet<ArtisanProfile> ArtisanProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InfluencerPost> InfluencerPosts { get; set; }
        public DbSet<PostTag> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            
            modelBuilder.Entity<User>()
                .HasOne(u => u.ArtisanProfile)
                .WithOne(ap => ap.User)
                .HasForeignKey<ArtisanProfile>(ap => ap.UserId);

            
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Material)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MaterialId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.InfluencerPost)
                .WithMany(ip => ip.Tags)
                .HasForeignKey(pt => pt.InfluencerPostId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.TaggedArtisanProfile)
                .WithMany(ap => ap.PostTags)
                .HasForeignKey(pt => pt.TaggedArtisanProfileId)
                .OnDelete(DeleteBehavior.Restrict); 

            
            
            modelBuilder.Entity<User>().ToTable("Users"); 
            modelBuilder.Entity<Role>().ToTable("Roles"); 
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");


            
            SeedCategories(modelBuilder);
            
        }

        private void SeedCategories(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Handmade Clothing", Description = "Locally crafted garments." },
                new Category { Id = 2, Name = "Woven Textiles", Description = "Traditional Myanmar woven fabrics." },
                new Category { Id = 3, Name = "Local Silk", Description = "Silk products sourced and made in Myanmar." },
                new Category { Id = 4, Name = "Artisan Bags", Description = "Handcrafted bags by local artisans." },
                new Category { Id = 5, Name = "Craft Supplies", Description = "Materials for crafting." }
            );
        }

    }
}