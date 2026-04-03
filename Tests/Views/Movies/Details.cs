using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieMVC.Controllers;
using MovieMVC.Data;
using MovieMVC.Models;
using MovieMVC.Repositories;
using MovieMVC.Services;
using Tests.Views.Home;
using Xunit;

namespace Tests.Views.Movies
{
    public class Details
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesDetails_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MoviesController CreateController(AppDbContext context)
        {
            var repo = new MovieRepository(context);
            var service = new MovieService(repo);
            var controller = new MoviesController(service, NullLogger<MoviesController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            return controller;
        }

        private void SeedData(AppDbContext context)
        {
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 4, Position = "Actor" });
            context.Actors.Add(new NamesLU { Id = 1, Name = "Alice" });
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });

            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test film" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WhenMovieExists()
        {
            using var context = CreateContext(nameof(Details_ReturnsViewResult_WhenMovieExists));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsMovieModel()
        {
            using var context = CreateContext(nameof(Details_ReturnsMovieModel));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Movie>(result.Model);
        }

        [Fact]
        public async Task Details_ModelContainsCorrectTitle()
        {
            using var context = CreateContext(nameof(Details_ModelContainsCorrectTitle));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = result!.Model as Movie;

            Assert.Equal("Test Movie", model!.Title);
        }

        [Fact]
        public async Task Details_ModelContainsDescription()
        {
            using var context = CreateContext(nameof(Details_ModelContainsDescription));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = result!.Model as Movie;

            Assert.Equal("A test film", model!.Description);
        }

        [Fact]
        public async Task Details_ModelContainsPeople()
        {
            using var context = CreateContext(nameof(Details_ModelContainsPeople));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = result!.Model as Movie;

            Assert.NotNull(model!.MoviePeople);
            Assert.Single(model.MoviePeople);
            Assert.Equal("Alice", model.MoviePeople[0].Name!.Name);
        }

        [Fact]
        public async Task Details_ModelContainsCategories()
        {
            using var context = CreateContext(nameof(Details_ModelContainsCategories));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = result!.Model as Movie;

            Assert.NotNull(model!.MovieCategory);
            Assert.Single(model.MovieCategory);
            Assert.Equal("Drama", model.MovieCategory[0].Category!.Category);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenMovieMissing()
        {
            using var context = CreateContext(nameof(Details_ReturnsNotFound_WhenMovieMissing));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Details(999, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
