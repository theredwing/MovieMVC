using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using Xunit;

namespace Tests.Repositories
{
    public class LookupRepository
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"LookupRepo_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private void SeedPositions(AppDbContext context)
        {
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 2, Position = "Actor" });
            context.SaveChanges();
        }

        // ── GetPositionId (sync) ──

        [Fact]
        public void GetPositionId_ReturnsId_WhenPositionExists()
        {
            using var context = CreateContext(nameof(GetPositionId_ReturnsId_WhenPositionExists));
            SeedPositions(context);
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var result = repo.GetPositionId("director");

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetPositionId_ReturnsZero_WhenPositionNotFound()
        {
            using var context = CreateContext(nameof(GetPositionId_ReturnsZero_WhenPositionNotFound));
            SeedPositions(context);
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var result = repo.GetPositionId("editor");

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetPositionId_IsCaseInsensitive()
        {
            using var context = CreateContext(nameof(GetPositionId_IsCaseInsensitive));
            SeedPositions(context);
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var result = repo.GetPositionId("director");

            Assert.Equal(1, result);
        }

        // ── GetPositionIdAsync ──

        [Fact]
        public async Task GetPositionIdAsync_ReturnsId_WhenExists()
        {
            using var context = CreateContext(nameof(GetPositionIdAsync_ReturnsId_WhenExists));
            SeedPositions(context);
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var id = await repo.GetPositionIdAsync("actor");

            Assert.Equal(2, id);
        }

        [Fact]
        public async Task GetPositionIdAsync_ReturnsZero_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetPositionIdAsync_ReturnsZero_WhenNotFound));
            SeedPositions(context);
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var id = await repo.GetPositionIdAsync("nonexistent");

            Assert.Equal(0, id);
        }

        // ── GetAllNames ──

        [Fact]
        public void GetAllNames_ReturnsOrderedNames()
        {
            using var context = CreateContext(nameof(GetAllNames_ReturnsOrderedNames));
            context.Actors.Add(new NamesLU { Id = 2, Name = "Zara" });
            context.Actors.Add(new NamesLU { Id = 3, Name = "Alice" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var names = repo.GetAllNames();

            Assert.Equal(2, names.Count);
            Assert.Equal("Alice", names[0].Name);
            Assert.Equal("Zara", names[1].Name);
        }

        // ── GetAllNamesWithMovieCount ──

        [Fact]
        public void GetAllNamesWithMovieCount_ReturnsNamesWithMoviePeople()
        {
            using var context = CreateContext(nameof(GetAllNamesWithMovieCount_ReturnsNamesWithMoviePeople));
            SeedPositions(context);
            var name = new NamesLU { Id = 1, Name = "John Doe" };
            context.Actors.Add(name);
            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test movie" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var names = repo.GetAllNamesWithMovieCount();

            Assert.Single(names);
            Assert.NotNull(names[0].MoviePeople);
            Assert.Single(names[0].MoviePeople);
        }

        // ── GetAllCategories ──

        [Fact]
        public void GetAllCategories_ReturnsOrderedCategories()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsOrderedCategories));
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Sci-Fi" });
            context.Categories.Add(new CategoryLU { Id = 2, Category = "Action" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var categories = repo.GetAllCategories();

            Assert.Equal(2, categories.Count);
            Assert.Equal("Action", categories[0].Category);
            Assert.Equal("Sci-Fi", categories[1].Category);
        }

        [Fact]
        public void GetAllCategories_ReturnsAllCategories()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsAllCategories));
            context.Categories.AddRange(
                new CategoryLU { Id = 1, Category = "Action" },
                new CategoryLU { Id = 2, Category = "Drama" },
                new CategoryLU { Id = 3, Category = "Comedy" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var result = repo.GetAllCategories();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAllCategories_ReturnsEmpty_WhenNoCategoriesExist()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsEmpty_WhenNoCategoriesExist));
            var repo = new MovieMVC.Repositories.LookupRepository(context);

            var result = repo.GetAllCategories();

            Assert.Empty(result);
        }
    }
}
