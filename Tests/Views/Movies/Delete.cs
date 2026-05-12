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
    public class Delete
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesDelete_{dbName}")
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
            context.Actors.Add(new NamesLU { Id = 1, Name = "Alice" });
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });

            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test film" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WhenMovieExists()
        {
            using var context = CreateContext(nameof(Delete_Get_ReturnsViewResult_WhenMovieExists));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Delete(1, null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsMovieModel()
        {
            using var context = CreateContext(nameof(Delete_Get_ReturnsMovieModel));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Delete(1, null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<MovieDetailsViewModel>(result.Model);
        }

        [Fact]
        public async Task Delete_Get_ModelContainsCorrectTitle()
        {
            using var context = CreateContext(nameof(Delete_Get_ModelContainsCorrectTitle));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Delete(1, null, null, null) as ViewResult;
            var viewModel = result!.Model as MovieDetailsViewModel;

            Assert.Equal("Test Movie", viewModel!.Movie.Title);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenMovieMissing()
        {
            using var context = CreateContext(nameof(Delete_Get_ReturnsNotFound_WhenMovieMissing));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Delete(999, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToHome_WhenDeleted()
        {
            using var context = CreateContext(nameof(DeleteConfirmed_RedirectsToHome_WhenDeleted));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.DeleteConfirmed(1, null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesMovieFromDatabase()
        {
            using var context = CreateContext(nameof(DeleteConfirmed_RemovesMovieFromDatabase));
            SeedData(context);
            var controller = CreateController(context);

            await controller.DeleteConfirmed(1, null, null, null);

            Assert.Empty(context.Movies);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsNotFound_WhenMovieMissing()
        {
            using var context = CreateContext(nameof(DeleteConfirmed_ReturnsNotFound_WhenMovieMissing));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.DeleteConfirmed(999, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
