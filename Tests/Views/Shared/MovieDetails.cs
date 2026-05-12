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
    public class MovieDetails
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"SharedMovieDetails_{dbName}")
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

        private void SeedFullMovie(AppDbContext context)
        {
            context.Positions.AddRange(
                new PositionLU { Id = 1, Position = "Director" },
                new PositionLU { Id = 2, Position = "Producer" },
                new PositionLU { Id = 3, Position = "Writer" },
                new PositionLU { Id = 4, Position = "Actor" });
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Alice Director" },
                new NamesLU { Id = 2, Name = "Bob Producer" },
                new NamesLU { Id = 3, Name = "Charlie Writer" },
                new NamesLU { Id = 4, Name = "Diana Actor" },
                new NamesLU { Id = 5, Name = "Eve Director2" });
            context.Categories.AddRange(
                new CategoryLU { Id = 1, Category = "Drama" },
                new CategoryLU { Id = 2, Category = "Action" });

            var movie = new Movie { Id = 1, Title = "Full Movie", Description = "A complete test film" };
            context.Movies.Add(movie);
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 5, PositionId = 1 },
                new MovieNamesPosition { Id = 3, MovieId = 1, NamesId = 2, PositionId = 2 },
                new MovieNamesPosition { Id = 4, MovieId = 1, NamesId = 3, PositionId = 3 },
                new MovieNamesPosition { Id = 5, MovieId = 1, NamesId = 4, PositionId = 4 });
            context.MovieCategories.AddRange(
                new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 },
                new MovieCategory { Id = 2, MovieId = 1, CategoryId = 2 });
            context.SaveChanges();
        }

        [Fact]
        public async Task Model_HasDirectorsWithNames()
        {
            using var context = CreateContext(nameof(Model_HasDirectorsWithNames));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            var directors = model.MoviePeople!.Where(mp => mp.PositionId == 1).ToList();
            Assert.Equal(2, directors.Count);
            Assert.Contains(directors, d => d.Name!.Name == "Alice Director");
            Assert.Contains(directors, d => d.Name!.Name == "Eve Director2");
        }

        [Fact]
        public async Task Model_HasProducersWithNames()
        {
            using var context = CreateContext(nameof(Model_HasProducersWithNames));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            var producers = model.MoviePeople!.Where(mp => mp.PositionId == 2).ToList();
            Assert.Single(producers);
            Assert.Equal("Bob Producer", producers[0].Name!.Name);
        }

        [Fact]
        public async Task Model_HasWritersWithNames()
        {
            using var context = CreateContext(nameof(Model_HasWritersWithNames));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            var writers = model.MoviePeople!.Where(mp => mp.PositionId == 3).ToList();
            Assert.Single(writers);
            Assert.Equal("Charlie Writer", writers[0].Name!.Name);
        }

        [Fact]
        public async Task Model_HasActorsWithNames()
        {
            using var context = CreateContext(nameof(Model_HasActorsWithNames));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            var actors = model.MoviePeople!.Where(mp => mp.PositionId == 4).ToList();
            Assert.Single(actors);
            Assert.Equal("Diana Actor", actors[0].Name!.Name);
        }

        [Fact]
        public async Task Model_HasCategoriesWithNames()
        {
            using var context = CreateContext(nameof(Model_HasCategoriesWithNames));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            var categories = model.MovieCategory!;
            Assert.Equal(2, categories.Count);
            Assert.Contains(categories, c => c.Category!.Category == "Drama");
            Assert.Contains(categories, c => c.Category!.Category == "Action");
        }

        [Fact]
        public async Task Model_HasTitleAndDescription()
        {
            using var context = CreateContext(nameof(Model_HasTitleAndDescription));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            Assert.Equal("Full Movie", model.Title);
            Assert.Equal("A complete test film", model.Description);
        }

        [Fact]
        public async Task Model_HandlesMovieWithNoPeople()
        {
            using var context = CreateContext(nameof(Model_HandlesMovieWithNoPeople));
            context.Movies.Add(new Movie { Id = 1, Title = "Lonely Movie" });
            context.SaveChanges();
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            Assert.NotNull(model.MoviePeople);
            Assert.Empty(model.MoviePeople);
        }

        [Fact]
        public async Task Model_HandlesMovieWithNoCategories()
        {
            using var context = CreateContext(nameof(Model_HandlesMovieWithNoCategories));
            context.Movies.Add(new Movie { Id = 1, Title = "Uncategorized Movie" });
            context.SaveChanges();
            var controller = CreateController(context);

            var result = await controller.Details(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            Assert.NotNull(model.MovieCategory);
            Assert.Empty(model.MovieCategory);
        }

        [Fact]
        public async Task DeleteView_ModelHasSameStructure()
        {
            using var context = CreateContext(nameof(DeleteView_ModelHasSameStructure));
            SeedFullMovie(context);
            var controller = CreateController(context);

            var result = await controller.Delete(1, null, null, null) as ViewResult;
            var model = (result!.Model as MovieDetailsViewModel)!.Movie;

            Assert.NotNull(model.MoviePeople);
            Assert.Equal(5, model.MoviePeople.Count);
            Assert.NotNull(model.MovieCategory);
            Assert.Equal(2, model.MovieCategory.Count);
        }
    }
}
