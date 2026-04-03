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
using Tests.Views.Home;
using Xunit;

namespace Tests.Views.Movies
{
    public class Edit
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesEdit_{dbName}")
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
                new PositionLU { Id = 2, Position = "Producer" },
                new PositionLU { Id = 3, Position = "Writer" },
                new PositionLU { Id = 4, Position = "Actor" });
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Alice" },
                new NamesLU { Id = 2, Name = "Bob" });
            context.Categories.AddRange(
                new CategoryLU { Id = 1, Category = "Drama" },
                new CategoryLU { Id = 2, Category = "Action" });

            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test film" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WhenMovieExists()
        {
            using var context = CreateContext(nameof(Edit_Get_ReturnsViewResult_WhenMovieExists));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Edit(1, null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsMovieFormViewModel()
        {
            using var context = CreateContext(nameof(Edit_Get_ReturnsMovieFormViewModel));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Edit(1, null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<MovieFormViewModel>(result.Model);
        }

        [Fact]
        public async Task Edit_Get_ViewModelContainsMovieData()
        {
            using var context = CreateContext(nameof(Edit_Get_ViewModelContainsMovieData));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Edit(1, null, null, null) as ViewResult;
            var model = result!.Model as MovieFormViewModel;

            Assert.Equal(1, model!.Movie.Id);
            Assert.Equal("Test Movie", model.Movie.Title);
        }

        [Fact]
        public async Task Edit_Get_ViewModelContainsSelectLists()
        {
            using var context = CreateContext(nameof(Edit_Get_ViewModelContainsSelectLists));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Edit(1, null, null, null) as ViewResult;
            var model = result!.Model as MovieFormViewModel;

            Assert.NotNull(model!.Directors);
            Assert.NotNull(model.Producers);
            Assert.NotNull(model.Writers);
            Assert.NotNull(model.Actors);
            Assert.NotNull(model.Categories);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenMovieMissing()
        {
            using var context = CreateContext(nameof(Edit_Get_ReturnsNotFound_WhenMovieMissing));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.Edit(999, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToDetails_WhenValid()
        {
            using var context = CreateContext(nameof(Edit_Post_RedirectsToDetails_WhenValid));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 1, Title = "Updated Movie", Description = "Updated" };

            var result = await controller.Edit(1, movie, [2], null, null, null, [2], null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_UpdatesMovieInDatabase()
        {
            using var context = CreateContext(nameof(Edit_Post_UpdatesMovieInDatabase));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 1, Title = "Updated Movie", Description = "Updated" };

            await controller.Edit(1, movie, null, null, null, null, null, null, null, null);

            var dbMovie = await context.Movies.FindAsync(1);
            Assert.Equal("Updated Movie", dbMovie!.Title);
            Assert.Equal("Updated", dbMovie.Description);
        }

        [Fact]
        public async Task Edit_Post_ReturnsBadRequest_WhenIdMismatch()
        {
            using var context = CreateContext(nameof(Edit_Post_ReturnsBadRequest_WhenIdMismatch));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 2, Title = "Mismatch" };

            var result = await controller.Edit(1, movie, null, null, null, null, null, null, null, null);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenMovieMissing()
        {
            using var context = CreateContext(nameof(Edit_Post_ReturnsNotFound_WhenMovieMissing));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 999, Title = "Missing" };

            var result = await controller.Edit(999, movie, null, null, null, null, null, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelStateInvalid()
        {
            using var context = CreateContext(nameof(Edit_Post_ReturnsView_WhenModelStateInvalid));
            SeedData(context);
            var controller = CreateController(context);
            controller.ModelState.AddModelError("Title", "Required");
            var movie = new Movie { Id = 1 };

            var result = await controller.Edit(1, movie, null, null, null, null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MovieFormViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ReplacesDirector()
        {
            using var context = CreateContext(nameof(Edit_Post_ReplacesDirector));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 1, Title = "Test Movie" };

            await controller.Edit(1, movie, [2], null, null, null, null, null, null, null);

            var people = context.MoviePeople.Where(mp => mp.MovieId == 1 && mp.PositionId == 1).ToList();
            Assert.Single(people);
            Assert.Equal(2, people[0].NamesId);
        }
    }
}
