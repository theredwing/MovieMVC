using Microsoft.EntityFrameworkCore;
using MovieMVC.Models;

namespace MovieMVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<NamesLU> Actors => Set<NamesLU>();
        public DbSet<PositionLU> Positions => Set<PositionLU>();
        public DbSet<CategoryLU> Categories => Set<CategoryLU>();
        public DbSet<MovieNamesPosition> MoviePeople => Set<MovieNamesPosition>();
        public DbSet<MovieCategory> MovieCategories => Set<MovieCategory>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // map CLR types to existing DB table names
            mb.Entity<CategoryLU>().ToTable("CategoryLU");
            mb.Entity<Movie>().ToTable("Movie");
            mb.Entity<NamesLU>().ToTable("NamesLU");
            mb.Entity<PositionLU>().ToTable("PositionLU");
            mb.Entity<MovieNamesPosition>().ToTable("MovieNamesPosition");
            mb.Entity<MovieCategory>().ToTable("MovieCategory");

            // Use Id as PK; unique index allows same Name with different Positions per Movie
            mb.Entity<MovieNamesPosition>().HasKey(ma => ma.Id);
            mb.Entity<MovieNamesPosition>()
                .HasIndex(ma => new { ma.MovieId, ma.NamesId, ma.PositionId }).IsUnique();

            mb.Entity<MovieNamesPosition>()
                .HasOne(ma => ma.Movie).WithMany(m => m.MoviePeople).HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<MovieNamesPosition>()
                .HasOne(ma => ma.Name).WithMany(a => a.MoviePeople).HasForeignKey(ma => ma.NamesId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<MovieNamesPosition>()
                .HasOne(ma => ma.Position).WithMany(p => p.MoviePeople).HasForeignKey(ma => ma.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie -> MovieCategory (one movie has many MovieCategory join rows)
            mb.Entity<MovieCategory>()
                .HasOne(mc => mc.Movie).WithMany(m => m.MovieCategory).HasForeignKey(mc => mc.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // MovieCategory -> Category (one MovieCategory has one Category)
            mb.Entity<MovieCategory>()
                .HasOne(mc => mc.Category).WithMany(c => c.MovieCategories).HasForeignKey(mc => mc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // (Note: configured above to avoid duplicate/conflicting mappings)
        }
    }
}
