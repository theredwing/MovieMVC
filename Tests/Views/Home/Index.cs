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

namespace Tests.Views.Home
{
    public class Index
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"HomeIndex_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private HomeController CreateController(AppDbContext context)
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

        private void SeedMovies(AppDbContext context)
        {
            var director = new PositionLU { Id = 1, Position = "Director" };
            var actor = new PositionLU { Id = 4, Position = "Actor" };
            context.Positions.AddRange(director, actor);

            var category1 = new CategoryLU { Id = 1, Category = "Drama" };
            var category2 = new CategoryLU { Id = 2, Category = "Action" };
            context.Categories.AddRange(category1, category2);

            var name1 = new NamesLU { Id = 1, Name = "Alice" };
            var name2 = new NamesLU { Id = 2, Name = "Bob" };
            context.Actors.AddRange(name1, name2);

            var movie1 = new Movie { Id = 1, Title = "Alpha Movie", Description = "A dramatic film" };
            var movie2 = new Movie { Id = 2, Title = "Beta Movie", Description = "An action film" };
            context.Movies.AddRange(movie1, movie2);

            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 2, NamesId = 2, PositionId = 4 });

            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 });

            context.SaveChanges();
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(Index_ReturnsViewResult));
            var controller = CreateController(context);

            var result = controller.Index(null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ReturnsMovieIndexViewModel()
        {
            using var context = CreateContext(nameof(Index_ReturnsMovieIndexViewModel));
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<MovieIndexViewModel>(result.Model);
        }

        [Fact]
        public void Index_ModelContainsAllMovies()
        {
            using var context = CreateContext(nameof(Index_ModelContainsAllMovies));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.NotNull(model);
            Assert.Equal(2, model.Movies.Count);
        }

        [Fact]
        public void Index_ModelMoviesContainExpectedTitles()
        {
            using var context = CreateContext(nameof(Index_ModelMoviesContainExpectedTitles));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Contains(model!.Movies, m => m.Title == "Alpha Movie");
            Assert.Contains(model.Movies, m => m.Title == "Beta Movie");
        }

        [Fact]
        public void Index_ModelMoviesContainDirectorsAndActors()
        {
            using var context = CreateContext(nameof(Index_ModelMoviesContainDirectorsAndActors));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            var alpha = model!.Movies.First(m => m.Title == "Alpha Movie");
            var beta = model.Movies.First(m => m.Title == "Beta Movie");

            Assert.Contains("Alice", alpha.Directors);
            Assert.Contains("Bob", beta.Actors);
        }

        [Fact]
        public void Index_ModelMoviesContainCategories()
        {
            using var context = CreateContext(nameof(Index_ModelMoviesContainCategories));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            var alpha = model!.Movies.First(m => m.Title == "Alpha Movie");
            var beta = model.Movies.First(m => m.Title == "Beta Movie");

            Assert.Contains("Drama", alpha.Categories);
            Assert.Contains("Action", beta.Categories);
        }

        [Fact]
        public void Index_SetsCurrentSort()
        {
            using var context = CreateContext(nameof(Index_SetsCurrentSort));
            var controller = CreateController(context);

            var result = controller.Index(null, "title") as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Equal("title", model!.CurrentSort);
        }

        [Fact]
        public void Index_SetsNextSortToTrue_WhenDescIsFalse()
        {
            using var context = CreateContext(nameof(Index_SetsNextSortToTrue_WhenDescIsFalse));
            var controller = CreateController(context);

            var result = controller.Index(null, null, desc: false) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.True(model!.NextSort);
        }

        [Fact]
        public void Index_SetsNextSortToFalse_WhenDescIsTrue()
        {
            using var context = CreateContext(nameof(Index_SetsNextSortToFalse_WhenDescIsTrue));
            var controller = CreateController(context);

            var result = controller.Index(null, null, desc: true) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.False(model!.NextSort);
        }

        [Fact]
        public void Index_SetsSearchOnModel()
        {
            using var context = CreateContext(nameof(Index_SetsSearchOnModel));
            var controller = CreateController(context);

            var result = controller.Index("test", null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Equal("test", model!.Search);
        }

        [Fact]
        public void Index_SearchFiltersMovies()
        {
            using var context = CreateContext(nameof(Index_SearchFiltersMovies));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index("Alpha", null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Single(model!.Movies);
            Assert.Equal("Alpha Movie", model.Movies[0].Title);
        }

        [Fact]
        public void Index_SearchReturnsEmpty_WhenNoMatch()
        {
            using var context = CreateContext(nameof(Index_SearchReturnsEmpty_WhenNoMatch));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index("NonExistent", null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Empty(model!.Movies);
        }

        [Fact]
        public void Index_SortsByTitle()
        {
            using var context = CreateContext(nameof(Index_SortsByTitle));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, "title") as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Equal("Alpha Movie", model!.Movies[0].Title);
            Assert.Equal("Beta Movie", model.Movies[1].Title);
        }

        [Fact]
        public void Index_SortsByTitleDescending()
        {
            using var context = CreateContext(nameof(Index_SortsByTitleDescending));
            SeedMovies(context);
            var controller = CreateController(context);

            var result = controller.Index(null, "title", desc: true) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Equal("Beta Movie", model!.Movies[0].Title);
            Assert.Equal("Alpha Movie", model.Movies[1].Title);
        }

        [Fact]
        public void Index_ReturnsEmptyMovies_WhenNoneExist()
        {
            using var context = CreateContext(nameof(Index_ReturnsEmptyMovies_WhenNoneExist));
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;
            var model = result!.Model as MovieIndexViewModel;

            Assert.Empty(model!.Movies);
        }

        [Fact]
        public void Index_SetsViewDataTitle()
        {
            using var context = CreateContext(nameof(Index_SetsViewDataTitle));
            var controller = CreateController(context);

            var result = controller.Index(null, null) as ViewResult;

            Assert.Equal("Home Page", result!.ViewData["Title"]);
        }
    }
}
