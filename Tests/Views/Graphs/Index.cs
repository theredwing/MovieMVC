using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieMVC.Controllers;
using MovieMVC.Data;
using MovieMVC.Models;
using MovieMVC.Models.ViewModels;
using MovieMVC.Repositories;
using MovieMVC.Services;
using Tests.TestHelpers;
using Xunit;

namespace Tests.Views.Graphs
{
    public class Index
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GraphsIndex_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private GraphsController CreateController(AppDbContext context)
        {
            var repo = new GraphRepository(context);
            var lookupRepo = new LookupRepository(context);
            var service = new GraphService(repo, lookupRepo);
            var controller = new GraphsController(service, NullLogger<GraphsController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            return controller;
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

            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 2, MovieId = 2, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 3, MovieId = 3, NamesId = 2, PositionId = 1 },
                new MovieNamesPosition { Id = 4, MovieId = 1, NamesId = 2, PositionId = 2 });

            context.MovieCategories.AddRange(
                new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 },
                new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 },
                new MovieCategory { Id = 3, MovieId = 3, CategoryId = 1 },
                new MovieCategory { Id = 4, MovieId = 3, CategoryId = 2 });

            context.SaveChanges();
        }

        // ── Basic view result ──

        [Fact]
        public void Index_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(Index_ReturnsViewResult));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ReturnsGraphViewModel()
        {
            using var context = CreateContext(nameof(Index_ReturnsGraphViewModel));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<GraphViewModel>(result.Model);
        }

        [Fact]
        public void Index_SetsViewDataTitle()
        {
            using var context = CreateContext(nameof(Index_SetsViewDataTitle));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, null, null) as ViewResult;

            Assert.NotNull(result);
        }

        // ── No type selected ──

        [Fact]
        public void Index_ReturnsEmptyModel_WhenNoTypeSelected()
        {
            using var context = CreateContext(nameof(Index_ReturnsEmptyModel_WhenNoTypeSelected));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Null(model.SelectedType);
            Assert.Empty(model.AvailableItems);
        }

        // ── Categories type ──

        [Fact]
        public void Index_LoadsCategoryDropdown_WhenTypeIsCategories()
        {
            using var context = CreateContext(nameof(Index_LoadsCategoryDropdown_WhenTypeIsCategories));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Categories", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Equal(2, model.AvailableItems.Count);
        }

        [Fact]
        public void Index_ShowsLabelsAndCounts_WhenCategoryIdsSelected()
        {
            using var context = CreateContext(nameof(Index_ShowsLabelsAndCounts_WhenCategoryIdsSelected));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Categories", [1, 2], null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Equal(2, model.Labels.Count);
            Assert.Equal(2, model.Counts.Count);
        }

        // ── Directors type ──

        [Fact]
        public void Index_LoadsDirectorDropdown_WhenTypeIsDirectors()
        {
            using var context = CreateContext(nameof(Index_LoadsDirectorDropdown_WhenTypeIsDirectors));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Directors", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Equal(2, model.AvailableItems.Count);
        }

        [Fact]
        public void Index_ShowsDirectorCounts_WhenDirectorIdsSelected()
        {
            using var context = CreateContext(nameof(Index_ShowsDirectorCounts_WhenDirectorIdsSelected));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Directors", [1, 2], null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            var aliceIdx = model.Labels.IndexOf("Alice");
            var bobIdx = model.Labels.IndexOf("Bob");
            Assert.Equal(2, model.Counts[aliceIdx]);
            Assert.Equal(1, model.Counts[bobIdx]);
        }

        // ── Actors type ──

        [Fact]
        public void Index_LoadsActorDropdown_WhenTypeIsActors()
        {
            using var context = CreateContext(nameof(Index_LoadsActorDropdown_WhenTypeIsActors));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Actors", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Single(model.AvailableItems);
        }

        // ── Sort/Desc/Search passthrough ──

        [Fact]
        public void Index_PassesSortToModel()
        {
            using var context = CreateContext(nameof(Index_PassesSortToModel));
            var controller = CreateController(context);

            var result = controller.Index(null, null, "title", null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.Equal("title", model!.Sort);
        }

        [Fact]
        public void Index_PassesDescToModel()
        {
            using var context = CreateContext(nameof(Index_PassesDescToModel));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, true, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.True(model!.Desc);
        }

        [Fact]
        public void Index_PassesSearchToModel()
        {
            using var context = CreateContext(nameof(Index_PassesSearchToModel));
            var controller = CreateController(context);

            var result = controller.Index(null, null, null, null, "test") as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.Equal("test", model!.Search);
        }

        // ── Selected type preserved ──

        [Fact]
        public void Index_PreservesSelectedType()
        {
            using var context = CreateContext(nameof(Index_PreservesSelectedType));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Categories", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.Equal("Categories", model!.SelectedType);
        }

        // ── Selected ids preserved ──

        [Fact]
        public void Index_PreservesSelectedIds()
        {
            using var context = CreateContext(nameof(Index_PreservesSelectedIds));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Categories", [1, 2], null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.Equal([1, 2], model!.SelectedIds);
        }

        // ── No chart when no ids ──

        [Fact]
        public void Index_NoChart_WhenNoIdsSelected()
        {
            using var context = CreateContext(nameof(Index_NoChart_WhenNoIdsSelected));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.Index("Categories", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.Empty(model!.Labels);
        }

        // ── Error handling ──

        [Fact]
        public void Index_SetsErrorMessage_WhenExceptionOccurs()
        {
            using var context = CreateContext(nameof(Index_SetsErrorMessage_WhenExceptionOccurs));
            context.Dispose(); // force error
            var repo = new GraphRepository(context);
            var lookupRepo = new LookupRepository(context);
            var service = new GraphService(repo, lookupRepo);
            var controller = new GraphsController(service, NullLogger<GraphsController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var result = controller.Index("Categories", null, null, null, null) as ViewResult;

            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public void Index_ReturnsEmptyModel_WhenExceptionOccurs()
        {
            using var context = CreateContext(nameof(Index_ReturnsEmptyModel_WhenExceptionOccurs));
            context.Dispose(); // force error
            var repo = new GraphRepository(context);
            var lookupRepo = new LookupRepository(context);
            var service = new GraphService(repo, lookupRepo);
            var controller = new GraphsController(service, NullLogger<GraphsController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var result = controller.Index("Categories", null, null, null, null) as ViewResult;
            var model = result!.Model as GraphViewModel;

            Assert.NotNull(model);
            Assert.Empty(model.AvailableItems);
        }
    }
}
