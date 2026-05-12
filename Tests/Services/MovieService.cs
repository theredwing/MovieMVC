using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using MovieMVC.Repositories;
using Xunit;

namespace Tests.Services
{
    public class MovieService
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MovieService_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MovieMVC.Services.MovieService CreateService(AppDbContext context)
        {
            var repo = new MovieRepository(context);
            var lookupRepo = new MovieMVC.Repositories.LookupRepository(context);
            return new MovieMVC.Services.MovieService(repo, lookupRepo);
        }

        private void SeedPositions(AppDbContext context)
        {
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 2, Position = "Producer" },
                new PositionLU { Id = 3, Position = "Writer" },
                new PositionLU { Id = 4, Position = "Actor" }
            );
            context.SaveChanges();
        }

        private void SeedFullData(AppDbContext context)
        {
            SeedPositions(context);

            var category1 = new CategoryLU { Id = 1, Category = "Drama" };
            var category2 = new CategoryLU { Id = 2, Category = "Action" };
            context.Categories.AddRange(category1, category2);

            var name1 = new NamesLU { Id = 1, Name = "Alice" };
            var name2 = new NamesLU { Id = 2, Name = "Bob" };
            var name3 = new NamesLU { Id = 3, Name = "Charlie" };
            context.Actors.AddRange(name1, name2, name3);

            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test film" };
            context.Movies.Add(movie);

            // Alice=Director, Bob=Actor, Charlie=Writer
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 2, PositionId = 4 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 3, MovieId = 1, NamesId = 3, PositionId = 3 });

            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });

            context.SaveChanges();
        }

        // ── GetMovieDetailsAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetMovieDetailsAsync_ReturnsMovie_WhenExists()
        {
            using var context = CreateContext(nameof(GetMovieDetailsAsync_ReturnsMovie_WhenExists));
            SeedFullData(context);
            var service = CreateService(context);

            var movie = await service.GetMovieDetailsAsync(1);

            Assert.NotNull(movie);
            Assert.Equal("Test Movie", movie.Title);
            Assert.NotNull(movie.MoviePeople);
            Assert.Equal(3, movie.MoviePeople.Count);
        }

        [Fact]
        public async Task GetMovieDetailsAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetMovieDetailsAsync_ReturnsNull_WhenNotFound));
            var service = CreateService(context);

            var movie = await service.GetMovieDetailsAsync(999);

            Assert.Null(movie);
        }

        // ── GetSelectedIdsAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetSelectedIdsAsync_ReturnsCorrectIds()
        {
            using var context = CreateContext(nameof(GetSelectedIdsAsync_ReturnsCorrectIds));
            SeedFullData(context);
            var service = CreateService(context);

            var movie = await context.Movies
                .Include(m => m.MoviePeople)
                .Include(m => m.MovieCategory)
                .FirstAsync(m => m.Id == 1);

            var selections = await service.GetSelectedIdsAsync(movie);

            Assert.Contains(1, selections.DirectorIds);
            Assert.Contains(2, selections.ActorIds);
            Assert.Contains(3, selections.WriterIds);
            Assert.Empty(selections.ProducerIds);
            Assert.Contains(1, selections.CategoryIds);
        }

        [Fact]
        public async Task GetSelectedIdsAsync_ReturnsEmptyArrays_WhenMovieHasNoPeople()
        {
            using var context = CreateContext(nameof(GetSelectedIdsAsync_ReturnsEmptyArrays_WhenMovieHasNoPeople));
            SeedPositions(context);
            context.Movies.Add(new Movie { Id = 1, Title = "Empty Movie" });
            context.SaveChanges();
            var service = CreateService(context);

            var movie = await context.Movies
                .Include(m => m.MoviePeople)
                .Include(m => m.MovieCategory)
                .FirstAsync(m => m.Id == 1);

            var selections = await service.GetSelectedIdsAsync(movie);

            Assert.Empty(selections.DirectorIds);
            Assert.Empty(selections.ProducerIds);
            Assert.Empty(selections.WriterIds);
            Assert.Empty(selections.ActorIds);
            Assert.Empty(selections.CategoryIds);
        }

        // ── CreateMovieAsync ─────────────────────────────────────────────

        [Fact]
        public async Task CreateMovieAsync_AddsMovieAndPeopleAndCategories()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_AddsMovieAndPeopleAndCategories));
            SeedPositions(context);
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Alice" },
                new NamesLU { Id = 2, Name = "Bob" }
            );
            context.SaveChanges();
            var service = CreateService(context);

            var movie = new Movie { Title = "New Movie", Description = "Desc" };
            await service.CreateMovieAsync(movie, [1], null, null, [2], [1]);

            Assert.Equal(1, await context.Movies.CountAsync());
            var saved = await context.Movies
                .Include(m => m.MoviePeople)
                .Include(m => m.MovieCategory)
                .FirstAsync();
            Assert.Equal("New Movie", saved.Title);
            Assert.Equal(2, saved.MoviePeople!.Count);
            Assert.Single(saved.MovieCategory!);
        }

        [Fact]
        public async Task CreateMovieAsync_HandlesNullSelections()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_HandlesNullSelections));
            SeedPositions(context);
            var service = CreateService(context);

            var movie = new Movie { Title = "Bare Movie" };
            await service.CreateMovieAsync(movie, null, null, null, null, null);

            Assert.Equal(1, await context.Movies.CountAsync());
            Assert.Equal(0, await context.MoviePeople.CountAsync());
            Assert.Equal(0, await context.MovieCategories.CountAsync());
        }

        [Fact]
        public async Task CreateMovieAsync_HandlesEmptySelections()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_HandlesEmptySelections));
            SeedPositions(context);
            var service = CreateService(context);

            var movie = new Movie { Title = "Bare Movie" };
            await service.CreateMovieAsync(movie, [], [], [], [], []);

            Assert.Equal(1, await context.Movies.CountAsync());
            Assert.Equal(0, await context.MoviePeople.CountAsync());
            Assert.Equal(0, await context.MovieCategories.CountAsync());
        }

        [Fact]
        public async Task CreateMovieAsync_DeduplicatesPeopleIds()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_DeduplicatesPeopleIds));
            SeedPositions(context);
            context.Actors.Add(new NamesLU { Id = 1, Name = "Alice" });
            context.SaveChanges();
            var service = CreateService(context);

            var movie = new Movie { Title = "Dup Movie" };
            await service.CreateMovieAsync(movie, [1, 1, 1], null, null, null, null);

            var people = await context.MoviePeople.ToListAsync();
            Assert.Single(people);
        }

        [Fact]
        public async Task CreateMovieAsync_DeduplicatesCategoryIds()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_DeduplicatesCategoryIds));
            SeedPositions(context);
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });
            context.SaveChanges();
            var service = CreateService(context);

            var movie = new Movie { Title = "Dup Cat Movie" };
            await service.CreateMovieAsync(movie, null, null, null, null, [1, 1, 1]);

            var cats = await context.MovieCategories.ToListAsync();
            Assert.Single(cats);
        }

        [Fact]
        public async Task CreateMovieAsync_AddsAllRoles()
        {
            using var context = CreateContext(nameof(CreateMovieAsync_AddsAllRoles));
            SeedPositions(context);
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Dir" },
                new NamesLU { Id = 2, Name = "Prod" },
                new NamesLU { Id = 3, Name = "Writ" },
                new NamesLU { Id = 4, Name = "Act" }
            );
            context.SaveChanges();
            var service = CreateService(context);

            var movie = new Movie { Title = "Full Movie" };
            await service.CreateMovieAsync(movie, [1], [2], [3], [4], null);

            var people = await context.MoviePeople.ToListAsync();
            Assert.Equal(4, people.Count);
            Assert.Contains(people, p => p.NamesId == 1 && p.PositionId == 1);
            Assert.Contains(people, p => p.NamesId == 2 && p.PositionId == 2);
            Assert.Contains(people, p => p.NamesId == 3 && p.PositionId == 3);
            Assert.Contains(people, p => p.NamesId == 4 && p.PositionId == 4);
        }

        // ── UpdateMovieAsync ─────────────────────────────────────────────

        [Fact]
        public async Task UpdateMovieAsync_ReturnsNull_WhenMovieNotFound()
        {
            using var context = CreateContext(nameof(UpdateMovieAsync_ReturnsNull_WhenMovieNotFound));
            var service = CreateService(context);

            var result = await service.UpdateMovieAsync(999, new Movie { Title = "X" }, null, null, null, null, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateMovieAsync_UpdatesTitleAndDescription()
        {
            using var context = CreateContext(nameof(UpdateMovieAsync_UpdatesTitleAndDescription));
            SeedFullData(context);
            var service = CreateService(context);

            var updated = new Movie { Title = "Updated Title", Description = "Updated Desc" };
            var result = await service.UpdateMovieAsync(1, updated, [1], null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated Desc", result.Description);
        }

        [Fact]
        public async Task UpdateMovieAsync_ReplacesAllPeople()
        {
            using var context = CreateContext(nameof(UpdateMovieAsync_ReplacesAllPeople));
            SeedFullData(context);
            var service = CreateService(context);

            // Originally: Alice=Director(1), Bob=Actor(2), Charlie=Writer(3)
            // Update to: only Bob=Director
            var updated = new Movie { Title = "Test Movie" };
            var result = await service.UpdateMovieAsync(1, updated, [2], null, null, null, null);

            Assert.NotNull(result);
            var people = await context.MoviePeople.Where(mp => mp.MovieId == 1).ToListAsync();
            Assert.Single(people);
            Assert.Equal(2, people[0].NamesId);
            Assert.Equal(1, people[0].PositionId);
        }

        [Fact]
        public async Task UpdateMovieAsync_ReplacesAllCategories()
        {
            using var context = CreateContext(nameof(UpdateMovieAsync_ReplacesAllCategories));
            SeedFullData(context);
            var service = CreateService(context);

            // Originally: category 1 (Drama). Update to: category 2 (Action)
            var updated = new Movie { Title = "Test Movie" };
            var result = await service.UpdateMovieAsync(1, updated, null, null, null, null, [2]);

            Assert.NotNull(result);
            var cats = await context.MovieCategories.Where(mc => mc.MovieId == 1).ToListAsync();
            Assert.Single(cats);
            Assert.Equal(2, cats[0].CategoryId);
        }

        [Fact]
        public async Task UpdateMovieAsync_ClearsAllPeopleAndCategories_WhenNullSelections()
        {
            using var context = CreateContext(nameof(UpdateMovieAsync_ClearsAllPeopleAndCategories_WhenNullSelections));
            SeedFullData(context);
            var service = CreateService(context);

            var updated = new Movie { Title = "Cleared Movie" };
            await service.UpdateMovieAsync(1, updated, null, null, null, null, null);

            Assert.Equal(0, await context.MoviePeople.CountAsync(mp => mp.MovieId == 1));
            Assert.Equal(0, await context.MovieCategories.CountAsync(mc => mc.MovieId == 1));
        }

        // ── DeleteMovieAsync ─────────────────────────────────────────────

        [Fact]
        public async Task DeleteMovieAsync_ReturnsTrue_AndRemovesMovie()
        {
            using var context = CreateContext(nameof(DeleteMovieAsync_ReturnsTrue_AndRemovesMovie));
            SeedFullData(context);
            var service = CreateService(context);

            var result = await service.DeleteMovieAsync(1);

            Assert.True(result);
            Assert.Equal(0, await context.Movies.CountAsync());
        }

        [Fact]
        public async Task DeleteMovieAsync_ReturnsFalse_WhenNotFound()
        {
            using var context = CreateContext(nameof(DeleteMovieAsync_ReturnsFalse_WhenNotFound));
            var service = CreateService(context);

            var result = await service.DeleteMovieAsync(999);

            Assert.False(result);
        }

        // ── GetAllNames / GetAllNamesWithMovieCount / GetAllCategories ───

        [Fact]
        public void GetAllNames_ReturnsOrderedNames()
        {
            using var context = CreateContext(nameof(GetAllNames_ReturnsOrderedNames));
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Zara" },
                new NamesLU { Id = 2, Name = "Alice" }
            );
            context.SaveChanges();
            var service = CreateService(context);

            var names = service.GetAllNames();

            Assert.Equal(2, names.Count);
            Assert.Equal("Alice", names[0].Name);
            Assert.Equal("Zara", names[1].Name);
        }

        [Fact]
        public void GetAllNamesWithMovieCount_ReturnsNamesWithMoviePeople()
        {
            using var context = CreateContext(nameof(GetAllNamesWithMovieCount_ReturnsNamesWithMoviePeople));
            SeedFullData(context);
            var service = CreateService(context);

            var names = service.GetAllNamesWithMovieCount();

            Assert.Equal(3, names.Count);
            Assert.All(names, n => Assert.NotNull(n.MoviePeople));
        }

        [Fact]
        public void GetAllCategories_ReturnsOrderedCategories()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsOrderedCategories));
            context.Categories.AddRange(
                new CategoryLU { Id = 3, Category = "Sci-Fi" },
                new CategoryLU { Id = 4, Category = "Action" }
            );
            context.SaveChanges();
            var service = CreateService(context);

            var categories = service.GetAllCategories();

            Assert.Equal(2, categories.Count);
            Assert.Equal("Action", categories[0].Category);
            Assert.Equal("Sci-Fi", categories[1].Category);
        }

        // ── MergeNamesAsync ─────────────────────────────────────────────

        [Fact]
        public async Task MergeNamesAsync_MergesSourceIntoTarget()
        {
            using var context = CreateContext(nameof(MergeNamesAsync_MergesSourceIntoTarget));
            SeedPositions(context);
            context.Actors.AddRange(
                new NamesLU { Id = 3, Name = "Target" },
                new NamesLU { Id = 4, Name = "Source" }
            );
            context.Movies.Add(new Movie { Id = 1, Title = "Movie1" });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 2, PositionId = 1 });
            context.SaveChanges();
            var service = CreateService(context);

            await service.MergeNamesAsync(1, [2]);

            Assert.Null(await context.Actors.FindAsync(2));
            var people = await context.MoviePeople.ToListAsync();
            Assert.Single(people);
            Assert.Equal(1, people[0].NamesId);
        }

        // ── Name CRUD ───────────────────────────────────────────────────

        [Fact]
        public async Task GetNameByIdAsync_ReturnsName_WhenExists()
        {
            using var context = CreateContext(nameof(GetNameByIdAsync_ReturnsName_WhenExists));
            context.Actors.Add(new NamesLU { Id = 1, Name = "Jane" });
            context.SaveChanges();
            var service = CreateService(context);

            var name = await service.GetNameByIdAsync(1);

            Assert.NotNull(name);
            Assert.Equal("Jane", name.Name);
        }

        [Fact]
        public async Task GetNameByIdAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetNameByIdAsync_ReturnsNull_WhenNotFound));
            var service = CreateService(context);

            var name = await service.GetNameByIdAsync(999);

            Assert.Null(name);
        }

        [Fact]
        public async Task NameExistsAsync_ReturnsTrue_WhenNameExists()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ReturnsTrue_WhenNameExists));
            context.Actors.Add(new NamesLU { Id = 5, Name = "John" });
            context.SaveChanges();
            var service = CreateService(context);

            var exists = await service.NameExistsAsync("john");

            Assert.True(exists);
        }

        [Fact]
        public async Task NameExistsAsync_ReturnsFalse_WhenNotFound()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ReturnsFalse_WhenNotFound));
            var service = CreateService(context);

            var exists = await service.NameExistsAsync("NonExistent");

            Assert.False(exists);
        }

        [Fact]
        public async Task NameExistsAsync_ExcludesId_WhenProvided()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ExcludesId_WhenProvided));
            context.Actors.Add(new NamesLU { Id = 1, Name = "John" });
            context.SaveChanges();
            var service = CreateService(context);

            var exists = await service.NameExistsAsync("john", excludeId: 1);

            Assert.False(exists);
        }

        [Fact]
        public async Task AddNameAsync_AddsAndSaves()
        {
            using var context = CreateContext(nameof(AddNameAsync_AddsAndSaves));
            var service = CreateService(context);

            await service.AddNameAsync(new NamesLU { Id = 1, Name = "NewName" });

            Assert.Equal(1, await context.Actors.CountAsync());
            Assert.Equal("NewName", (await context.Actors.FirstAsync()).Name);
        }

        [Fact]
        public async Task UpdateNameAsync_UpdatesAndSaves()
        {
            using var context = CreateContext(nameof(UpdateNameAsync_UpdatesAndSaves));
            context.Actors.Add(new NamesLU { Id = 6, Name = "OldName" });
            context.SaveChanges();
            var service = CreateService(context);

            var name = await context.Actors.FindAsync(6);
            name!.Name = "UpdatedName";
            await service.UpdateNameAsync(name);

            var updated = await context.Actors.FindAsync(6);
            Assert.Equal("UpdatedName", updated!.Name);
        }

        [Fact]
        public async Task DeleteNameAsync_ReturnsTrue_WhenDeleted()
        {
            using var context = CreateContext(nameof(DeleteNameAsync_ReturnsTrue_WhenDeleted));
            context.Actors.Add(new NamesLU { Id = 1, Name = "ToDelete" });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.DeleteNameAsync(1);

            Assert.True(result);
            Assert.Equal(0, await context.Actors.CountAsync());
        }

        [Fact]
        public async Task DeleteNameAsync_ReturnsFalse_WhenNotFound()
        {
            using var context = CreateContext(nameof(DeleteNameAsync_ReturnsFalse_WhenNotFound));
            var service = CreateService(context);

            var result = await service.DeleteNameAsync(999);

            Assert.False(result);
        }
    }
}
