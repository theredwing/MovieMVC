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
using Tests.TestHelpers;
using Xunit;

namespace Tests.Views.Shared
{
    public class SortSearchHidden
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"SharedSortSearch_{dbName}")
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
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 2, Position = "Producer" },
                new PositionLU { Id = 3, Position = "Writer" },
                new PositionLU { Id = 4, Position = "Actor" });
            context.Actors.Add(new NamesLU { Id = 1, Name = "Alice" });
            context.Categories.Add(new CategoryLU { Id = 1, Category = "Drama" });
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();
        }

        [Fact]
        public async Task CreatePost_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(CreatePost_RedirectIncludesSortDescSearch));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie" };

            var result = await controller.Create(movie, null, null, null, null, null, "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task EditPost_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(EditPost_RedirectIncludesSortDescSearch));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Id = 1, Title = "Updated" };

            var result = await controller.Edit(1, movie, null, null, null, null, null, "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(DeleteConfirmed_RedirectIncludesSortDescSearch));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.DeleteConfirmed(1, "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task AddName_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(AddName_RedirectIncludesSortDescSearch));
            var controller = CreateController(context);

            var result = await controller.AddName("New Name", "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task EditName_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(EditName_RedirectIncludesSortDescSearch));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.EditName(1, "Alice Updated", "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task DeleteName_RedirectIncludesSortDescSearch()
        {
            using var context = CreateContext(nameof(DeleteName_RedirectIncludesSortDescSearch));
            SeedData(context);
            var controller = CreateController(context);

            var result = await controller.DeleteName(1, "title", true, "test");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("title", redirect.RouteValues?["sort"]);
            Assert.Equal(true, redirect.RouteValues?["desc"]);
            Assert.Equal("test", redirect.RouteValues?["search"]);
        }

        [Fact]
        public async Task CreatePost_RedirectOmitsNull_SortDescSearch()
        {
            using var context = CreateContext(nameof(CreatePost_RedirectOmitsNull_SortDescSearch));
            SeedData(context);
            var controller = CreateController(context);
            var movie = new Movie { Title = "New Movie" };

            var result = await controller.Create(movie, null, null, null, null, null, null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirect.RouteValues?["sort"]);
            Assert.Null(redirect.RouteValues?["search"]);
        }
    }
}
