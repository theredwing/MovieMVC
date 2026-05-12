using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using MovieMVC.Repositories;
using Xunit;

namespace Tests.Services
{
    public class GraphService
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GraphService_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MovieMVC.Services.GraphService CreateService(AppDbContext context)
        {
            var repo = new GraphRepository(context);
            var lookupRepo = new MovieMVC.Repositories.LookupRepository(context);
            return new MovieMVC.Services.GraphService(repo, lookupRepo);
        }

        private void SeedData(AppDbContext context)
        {
            var director = new PositionLU { Id = 1, Position = "Director" };
            var actor = new PositionLU { Id = 2, Position = "Actor" };
            context.Positions.AddRange(director, actor);

            var cat1 = new CategoryLU { Id = 1, Category = "Action" };
            var cat2 = new CategoryLU { Id = 2, Category = "Drama" };
            context.Categories.AddRange(cat1, cat2);

            var name1 = new NamesLU { Id = 1, Name = "Alice" };
            var name2 = new NamesLU { Id = 2, Name = "Bob" };
            context.Actors.AddRange(name1, name2);

            var movie1 = new Movie { Id = 1, Title = "Movie A", Description = "Desc A" };
            var movie2 = new Movie { Id = 2, Title = "Movie B", Description = "Desc B" };
            var movie3 = new Movie { Id = 3, Title = "Movie C", Description = "Desc C" };
            context.Movies.AddRange(movie1, movie2, movie3);

            // Alice directed Movie A and Movie B
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 2, MovieId = 2, NamesId = 1, PositionId = 1 });

            // Bob directed Movie C, acted in Movie A
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 3, MovieId = 3, NamesId = 2, PositionId = 1 },
                new MovieNamesPosition { Id = 4, MovieId = 1, NamesId = 2, PositionId = 2 });

            // Movie A => Action, Movie B => Drama, Movie C => Action + Drama
            context.MovieCategories.AddRange(
                new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 },
                new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 },
                new MovieCategory { Id = 3, MovieId = 3, CategoryId = 1 },
                new MovieCategory { Id = 4, MovieId = 3, CategoryId = 2 });

            context.SaveChanges();
        }

        // ── BuildViewModel: null/empty type ──

        [Fact]
        public void BuildViewModel_ReturnsEmptyViewModel_WhenTypeIsNull()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsEmptyViewModel_WhenTypeIsNull));
            var service = CreateService(context);

            var result = service.BuildViewModel(null, null, null, null, null);

            Assert.Null(result.SelectedType);
            Assert.Empty(result.AvailableItems);
            Assert.Empty(result.Labels);
            Assert.Empty(result.Counts);
        }

        [Fact]
        public void BuildViewModel_ReturnsEmptyViewModel_WhenTypeIsEmpty()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsEmptyViewModel_WhenTypeIsEmpty));
            var service = CreateService(context);

            var result = service.BuildViewModel("", null, null, null, null);

            Assert.Empty(result.AvailableItems);
        }

        // ── BuildViewModel: sort/desc/search passthrough ──

        [Fact]
        public void BuildViewModel_SetsSortDescSearch()
        {
            using var context = CreateContext(nameof(BuildViewModel_SetsSortDescSearch));
            var service = CreateService(context);

            var result = service.BuildViewModel(null, null, "title", true, "test");

            Assert.Equal("title", result.Sort);
            Assert.True(result.Desc);
            Assert.Equal("test", result.Search);
        }

        // ── BuildViewModel: Categories ──

        [Fact]
        public void BuildViewModel_LoadsCategoryItems_WhenTypeIsCategories()
        {
            using var context = CreateContext(nameof(BuildViewModel_LoadsCategoryItems_WhenTypeIsCategories));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", null, null, null, null);

            Assert.Equal(2, result.AvailableItems.Count);
            Assert.Contains(result.AvailableItems, i => i.Text == "Action");
            Assert.Contains(result.AvailableItems, i => i.Text == "Drama");
        }

        [Fact]
        public void BuildViewModel_ReturnsLabelsAndCounts_WhenCategoryIdsProvided()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsLabelsAndCounts_WhenCategoryIdsProvided));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [1, 2], null, null, null);

            Assert.Equal(2, result.Labels.Count);
            Assert.Equal(2, result.Counts.Count);
            Assert.Contains("Action", result.Labels);
            Assert.Contains("Drama", result.Labels);
        }

        [Fact]
        public void BuildViewModel_ReturnsCategoryCounts_Correctly()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsCategoryCounts_Correctly));
            SeedData(context);
            var service = CreateService(context);

            // Action: Movie A + Movie C = 2, Drama: Movie B + Movie C = 2
            var result = service.BuildViewModel("Categories", [1, 2], null, null, null);

            var actionIdx = result.Labels.IndexOf("Action");
            var dramaIdx = result.Labels.IndexOf("Drama");
            Assert.Equal(2, result.Counts[actionIdx]);
            Assert.Equal(2, result.Counts[dramaIdx]);
        }

        // ── BuildViewModel: Directors (people) ──

        [Fact]
        public void BuildViewModel_LoadsDirectorItems_WhenTypeIsDirectors()
        {
            using var context = CreateContext(nameof(BuildViewModel_LoadsDirectorItems_WhenTypeIsDirectors));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Directors", null, null, null, null);

            Assert.Equal(2, result.AvailableItems.Count);
            Assert.Contains(result.AvailableItems, i => i.Text == "Alice");
            Assert.Contains(result.AvailableItems, i => i.Text == "Bob");
        }

        [Fact]
        public void BuildViewModel_ReturnsDirectorCounts_Correctly()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsDirectorCounts_Correctly));
            SeedData(context);
            var service = CreateService(context);

            // Alice directed 2 movies, Bob directed 1
            var result = service.BuildViewModel("Directors", [1, 2], null, null, null);

            var aliceIdx = result.Labels.IndexOf("Alice");
            var bobIdx = result.Labels.IndexOf("Bob");
            Assert.Equal(2, result.Counts[aliceIdx]);
            Assert.Equal(1, result.Counts[bobIdx]);
        }

        // ── BuildViewModel: Actors ──

        [Fact]
        public void BuildViewModel_LoadsActorItems_WhenTypeIsActors()
        {
            using var context = CreateContext(nameof(BuildViewModel_LoadsActorItems_WhenTypeIsActors));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Actors", null, null, null, null);

            Assert.Single(result.AvailableItems);
            Assert.Contains(result.AvailableItems, i => i.Text == "Bob");
        }

        [Fact]
        public void BuildViewModel_ReturnsActorCounts_Correctly()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsActorCounts_Correctly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Actors", [2], null, null, null);

            Assert.Single(result.Counts);
            Assert.Equal(1, result.Counts[0]);
        }

        // ── BuildViewModel: unknown type ──

        [Fact]
        public void BuildViewModel_ReturnsEmptyItems_WhenTypeIsUnknown()
        {
            using var context = CreateContext(nameof(BuildViewModel_ReturnsEmptyItems_WhenTypeIsUnknown));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Unknown", null, null, null, null);

            Assert.Empty(result.AvailableItems);
        }

        // ── BuildViewModel: id filtering ──

        [Fact]
        public void BuildViewModel_FiltersOutZeroIds()
        {
            using var context = CreateContext(nameof(BuildViewModel_FiltersOutZeroIds));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [0, 1], null, null, null);

            Assert.Single(result.Labels);
            Assert.Equal("Action", result.Labels[0]);
        }

        [Fact]
        public void BuildViewModel_DeduplicatesIds()
        {
            using var context = CreateContext(nameof(BuildViewModel_DeduplicatesIds));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [1, 1], null, null, null);

            Assert.Single(result.Labels);
        }

        [Fact]
        public void BuildViewModel_TakesMaxFourIds()
        {
            using var context = CreateContext(nameof(BuildViewModel_TakesMaxFourIds));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [1, 2, 1, 2, 1], null, null, null);

            Assert.True(result.SelectedIds!.Length <= 4);
        }

        [Fact]
        public void BuildViewModel_SetsSelectedIds()
        {
            using var context = CreateContext(nameof(BuildViewModel_SetsSelectedIds));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [1, 2], null, null, null);

            Assert.Equal([1, 2], result.SelectedIds);
        }

        [Fact]
        public void BuildViewModel_ShowsUnknown_WhenIdNotInAvailableItems()
        {
            using var context = CreateContext(nameof(BuildViewModel_ShowsUnknown_WhenIdNotInAvailableItems));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [99], null, null, null);

            Assert.Single(result.Labels);
            Assert.Equal("Unknown", result.Labels[0]);
            Assert.Equal(0, result.Counts[0]);
        }

        [Fact]
        public void BuildViewModel_NoLabelsOrCounts_WhenIdsNull()
        {
            using var context = CreateContext(nameof(BuildViewModel_NoLabelsOrCounts_WhenIdsNull));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", null, null, null, null);

            Assert.Empty(result.Labels);
            Assert.Empty(result.Counts);
        }

        [Fact]
        public void BuildViewModel_NoLabelsOrCounts_WhenIdsEmpty()
        {
            using var context = CreateContext(nameof(BuildViewModel_NoLabelsOrCounts_WhenIdsEmpty));
            SeedData(context);
            var service = CreateService(context);

            var result = service.BuildViewModel("Categories", [], null, null, null);

            Assert.Empty(result.Labels);
            Assert.Empty(result.Counts);
        }
    }
}
