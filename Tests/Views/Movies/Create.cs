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
    public class Create
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesCreate_{dbName}")
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

        private void SeedLookups(AppDbContext context)
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
            context.SaveChanges();
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(Create_Get_ReturnsViewResult));
            SeedLookups(context);
            var controller = CreateController(context);

            var result = controller.Create(null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsMovieFormViewModel()
        {
            using var context = CreateContext(nameof(Create_Get_ReturnsMovieFormViewModel));
            SeedLookups(context);
            var controller = CreateController(context);

            var result = controller.Create(null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<MovieFormViewModel>(result.Model);
        }

        [Fact]
        public void Create_Get_ViewModelContainsDirectorsList()
        {
            using var context = CreateContext(nameof(Create_Get_ViewModelContainsDirectorsList));
            SeedLookups(context);
            var controller = CreateController(context);

            var result = controller.Create(null, null, null) as ViewResult;
            var model = result!.Model as MovieFormViewModel;

            Assert.NotNull(model!.Directors);
            Assert.Equal(2, model.Directors.Count());
        }

        [Fact]
        public void Create_Get_ViewModelContainsCategoriesList()
        {
            using var context = CreateContext(nameof(Create_Get_ViewModelContainsCategoriesList));
            SeedLookups(context);
            var controller = CreateController(context);

            var result = controller.Create(null, null, null) as ViewResult;
            var model = result!.Model as MovieFormViewModel;

            Assert.NotNull(model!.Categories);
            Assert.Equal(2, model.Categories.Count());
        }

        [Fact]
        public void Create_Get_ViewModelMovieIsEmpty()
        {
            using var context = CreateContext(nameof(Create_Get_ViewModelMovieIsEmpty));
            SeedLookups(context);
            var controller = CreateController(context);

            var result = controller.Create(null, null, null) as ViewResult;
            var model = result!.Model as MovieFormViewModel;

            Assert.Equal(0, model!.Movie.Id);
            Assert.Equal("", model.Movie.Title);
        }

        [Fact]
        public async Task Create_Post_RedirectsToDetails_WhenValid()
        {
            using var context = CreateContext(nameof(Create_Post_RedirectsToDetails_WhenValid));
            SeedLookups(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie", Description = "Desc" };

            var result = await controller.Create(movie, [1], null, null, null, [1], null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_CreatesMovieInDatabase()
        {
            using var context = CreateContext(nameof(Create_Post_CreatesMovieInDatabase));
            SeedLookups(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie", Description = "Desc" };

            await controller.Create(movie, [1], null, null, null, [1], null, null, null);

            Assert.Single(context.Movies);
            Assert.Equal("New Movie", context.Movies.First().Title);
        }

        [Fact]
        public async Task Create_Post_AssignsDirectorToMovie()
        {
            using var context = CreateContext(nameof(Create_Post_AssignsDirectorToMovie));
            SeedLookups(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie" };

            await controller.Create(movie, [1], null, null, null, null, null, null, null);

            var mp = context.MoviePeople.FirstOrDefault();
            Assert.NotNull(mp);
            Assert.Equal(1, mp.NamesId);
            Assert.Equal(1, mp.PositionId);
        }

        [Fact]
        public async Task Create_Post_AssignsCategoryToMovie()
        {
            using var context = CreateContext(nameof(Create_Post_AssignsCategoryToMovie));
            SeedLookups(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie" };

            await controller.Create(movie, null, null, null, null, [1], null, null, null);

            var mc = context.MovieCategories.FirstOrDefault();
            Assert.NotNull(mc);
            Assert.Equal(1, mc.CategoryId);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateInvalid()
        {
            using var context = CreateContext(nameof(Create_Post_ReturnsView_WhenModelStateInvalid));
            SeedLookups(context);
            var controller = CreateController(context);
            controller.ModelState.AddModelError("Title", "Required");
            var movie = new Movie();

            var result = await controller.Create(movie, null, null, null, null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MovieFormViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_DoesNotCreateMovie_WhenModelStateInvalid()
        {
            using var context = CreateContext(nameof(Create_Post_DoesNotCreateMovie_WhenModelStateInvalid));
            SeedLookups(context);
            var controller = CreateController(context);
            controller.ModelState.AddModelError("Title", "Required");
            var movie = new Movie();

            await controller.Create(movie, null, null, null, null, null, null, null, null);

            Assert.Empty(context.Movies);
        }
    }
}
