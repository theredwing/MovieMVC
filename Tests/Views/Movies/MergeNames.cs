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

namespace Tests.Views.Movies
{
    public class MergeNames
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesMerge_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MoviesController CreateController(AppDbContext context)
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

        private void SeedData(AppDbContext context)
        {
            context.Positions.Add(new PositionLU { Id = 1, Position = "Director" });
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Alice" },
                new NamesLU { Id = 2, Name = "Alice Duplicate" },
                new NamesLU { Id = 3, Name = "Bob" });
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });

            var movie = new Movie { Id = 1, Title = "Test Movie" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 2, PositionId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public void MergeNames_Get_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(MergeNames_Get_ReturnsViewResult));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.MergeNames();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void MergeNames_Get_ReturnsMergeNamesViewModel()
        {
            using var context = CreateContext(nameof(MergeNames_Get_ReturnsMergeNamesViewModel));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.MergeNames() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<MergeNamesViewModel>(result.Model);
        }

        [Fact]
        public void MergeNames_Get_ViewModelContainsNamesList()
        {
            using var context = CreateContext(nameof(MergeNames_Get_ViewModelContainsNamesList));
            SeedData(context);
            var controller = CreateController(context);

            var result = controller.MergeNames() as ViewResult;
            var model = result!.Model as MergeNamesViewModel;

            Assert.NotNull(model!.Names);
            Assert.Equal(3, model.Names.Count());
        }

        [Fact]
        public async Task MergeNames_Post_RedirectsToHome_WhenValid()
        {
            using var context = CreateContext(nameof(MergeNames_Post_RedirectsToHome_WhenValid));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.MergeNames(1, 2, null, null, null, null, null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task MergeNames_Post_MergesMovieReferences()
        {
            using var context = CreateContext(nameof(MergeNames_Post_MergesMovieReferences));
            SeedData(context);
            var controller = CreateController(context);

            await controller.MergeNames(1, 2, null, null, null, null, null, null, null);

            var people = context.MoviePeople.Where(mp => mp.MovieId == 1).ToList();
            Assert.Single(people);
            Assert.Equal(1, people[0].NamesId);
        }

        [Fact]
        public async Task MergeNames_Post_DeletesSourceName()
        {
            using var context = CreateContext(nameof(MergeNames_Post_DeletesSourceName));
            SeedData(context);
            var controller = CreateController(context);

            await controller.MergeNames(1, 2, null, null, null, null, null, null, null);

            Assert.Null(await context.Actors.FindAsync(2));
            Assert.Equal(2, context.Actors.Count());
        }

        [Fact]
        public async Task MergeNames_Post_SetsSuccessMessage()
        {
            using var context = CreateContext(nameof(MergeNames_Post_SetsSuccessMessage));
            SeedData(context);
            var controller = CreateController(context);

            await controller.MergeNames(1, 2, null, null, null, null, null, null, null);

            Assert.Contains("merged successfully", controller.TempData["SuccessMessage"] as string);
        }

        [Fact]
        public async Task MergeNames_Post_ReturnsView_WhenTargetIdIsZero()
        {
            using var context = CreateContext(nameof(MergeNames_Post_ReturnsView_WhenTargetIdIsZero));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.MergeNames(0, 2, null, null, null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MergeNamesViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task MergeNames_Post_ReturnsView_WhenSourceIdIsZero()
        {
            using var context = CreateContext(nameof(MergeNames_Post_ReturnsView_WhenSourceIdIsZero));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.MergeNames(1, 0, null, null, null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MergeNamesViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task MergeNames_Post_ReturnsView_WhenAllSameIds()
        {
            using var context = CreateContext(nameof(MergeNames_Post_ReturnsView_WhenAllSameIds));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.MergeNames(1, 1, null, null, null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MergeNamesViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task MergeNames_Post_MergesMultipleSources()
        {
            using var context = CreateContext(nameof(MergeNames_Post_MergesMultipleSources));
            SeedData(context);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 3, PositionId = 1 });
            context.SaveChanges();
            var controller = CreateController(context);

            await controller.MergeNames(1, 2, 3, null, null, null, null, null, null);

            Assert.Equal(1, context.Actors.Count());
            Assert.NotNull(await context.Actors.FindAsync(1));
        }
    }
}
