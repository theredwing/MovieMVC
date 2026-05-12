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

namespace Tests.Views.Shared
{
    public class TempMessages
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"SharedTempMsg_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MoviesController CreateMoviesController(AppDbContext context)
        {
            var repo = new MovieRepository(context);
            var lookupRepo = new LookupRepository(context);
            var service = new MovieService(repo, lookupRepo);
            var controller = new MoviesController(service, NullLogger<MoviesController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            return controller;
        }

        private HomeController CreateHomeController(AppDbContext context)
        {
            var repo = new HomeRepository(context);
            var lookupRepo = new LookupRepository(context);
            var service = new HomeService(repo, lookupRepo);
            var controller = new HomeController(service, NullLogger<HomeController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            return controller;
        }

        private void SeedData(AppDbContext context)
        {
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 2, Position = "Producer" },
                new PositionLU { Id = 3, Position = "Writer" },
                new PositionLU { Id = 4, Position = "Actor" });
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Alice" },
                new NamesLU { Id = 2, Name = "Bob" });
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public void HomeIndex_DoesNotSetErrorMessage_OnSuccess()
        {
            using var context = CreateContext(nameof(HomeIndex_DoesNotSetErrorMessage_OnSuccess));
            var controller = CreateHomeController(context);

            controller.Index(null, null);

            Assert.Null(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public void HomeIndex_DoesNotSetSuccessMessage()
        {
            using var context = CreateContext(nameof(HomeIndex_DoesNotSetSuccessMessage));
            var controller = CreateHomeController(context);

            controller.Index(null, null);

            Assert.Null(controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public void MergeNamesGet_ClearsSuccessMessage()
        {
            using var context = CreateContext(nameof(MergeNamesGet_ClearsSuccessMessage));
            SeedData(context);
            var controller = CreateMoviesController(context);
            controller.TempData["SuccessMessage"] = "Previous message";

            controller.MergeNames();

            Assert.Null(controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task AddName_SetsOnlySuccessMessage_WhenValid()
        {
            using var context = CreateContext(nameof(AddName_SetsOnlySuccessMessage_WhenValid));
            var controller = CreateMoviesController(context);

            await controller.AddName("New Name", null, null, null);

            Assert.NotNull(controller.TempData["SuccessMessage"]);
            Assert.Null(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task AddName_SetsOnlyErrorMessage_WhenDuplicate()
        {
            using var context = CreateContext(nameof(AddName_SetsOnlyErrorMessage_WhenDuplicate));
            SeedData(context);
            var controller = CreateMoviesController(context);

            await controller.AddName("Alice", null, null, null);

            Assert.NotNull(controller.TempData["ErrorMessage"]);
            Assert.Null(controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task DeleteConfirmed_DoesNotSetErrorMessage_WhenSuccess()
        {
            using var context = CreateContext(nameof(DeleteConfirmed_DoesNotSetErrorMessage_WhenSuccess));
            SeedData(context);
            var controller = CreateMoviesController(context);

            await controller.DeleteConfirmed(1, null, null, null);

            Assert.Null(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task EditName_SetsErrorMessage_WhenNameEmpty()
        {
            using var context = CreateContext(nameof(EditName_SetsErrorMessage_WhenNameEmpty));
            SeedData(context);
            var controller = CreateMoviesController(context);

            await controller.EditName(1, "  ", null, null, null);

            Assert.Contains("cannot be empty", controller.TempData["ErrorMessage"] as string);
        }

        [Fact]
        public async Task EditName_SetsSuccessMessage_WhenValid()
        {
            using var context = CreateContext(nameof(EditName_SetsSuccessMessage_WhenValid));
            SeedData(context);
            var controller = CreateMoviesController(context);

            await controller.EditName(1, "Alice Updated", null, null, null);

            Assert.Contains("successfully", controller.TempData["SuccessMessage"] as string);
            Assert.Null(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task MergeNamesPost_SetsSuccessMessage_WhenValid()
        {
            using var context = CreateContext(nameof(MergeNamesPost_SetsSuccessMessage_WhenValid));
            SeedData(context);
            var controller = CreateMoviesController(context);

            await controller.MergeNames(1, 2, null, null, null, null, null, null, null);

            Assert.Contains("merged successfully", controller.TempData["SuccessMessage"] as string);
        }

        [Fact]
        public async Task CreatePost_DoesNotSetErrorMessage_WhenSuccess()
        {
            using var context = CreateContext(nameof(CreatePost_DoesNotSetErrorMessage_WhenSuccess));
            SeedData(context);
            var controller = CreateMoviesController(context);
            var movie = new Movie { Title = "New Movie" };

            await controller.Create(movie, [1], null, null, null, null, null, null, null);

            Assert.Null(controller.TempData["ErrorMessage"]);
        }
    }
}
