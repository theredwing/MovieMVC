using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using MovieMVC.Repositories;
using Xunit;

namespace Tests.Services
{
    public class HomeService
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"HomeService_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private MovieMVC.Services.HomeService CreateService(AppDbContext context)
        {
            var repo = new HomeRepository(context);
            return new MovieMVC.Services.HomeService(repo);
        }

        private void SeedData(AppDbContext context)
        {
            var director = new PositionLU { Id = 1, Position = "Director" };
            var producer = new PositionLU { Id = 2, Position = "Producer" };
            var writer = new PositionLU { Id = 3, Position = "Writer" };
            var actor = new PositionLU { Id = 4, Position = "Actor" };
            context.Positions.AddRange(director, producer, writer, actor);

            var category1 = new CategoryLU { Id = 1, Category = "Drama" };
            var category2 = new CategoryLU { Id = 2, Category = "Action" };
            context.Categories.AddRange(category1, category2);

            var name1 = new NamesLU { Id = 1, Name = "Alice Director" };
            var name2 = new NamesLU { Id = 2, Name = "Bob Actor" };
            var name3 = new NamesLU { Id = 3, Name = "Charlie Writer" };
            var name4 = new NamesLU { Id = 4, Name = "Zara Director" };
            var name5 = new NamesLU { Id = 5, Name = "Eve Producer" };
            context.Actors.AddRange(name1, name2, name3, name4, name5);

            var movie1 = new Movie { Id = 1, Title = "Beta Movie", Description = "A dramatic film" };
            var movie2 = new Movie { Id = 2, Title = "Alpha Movie", Description = "An action film" };
            context.Movies.AddRange(movie1, movie2);

            // Movie 1: director=Alice, actor=Bob, writer=Charlie, category=Drama
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 2, PositionId = 4 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 3, MovieId = 1, NamesId = 3, PositionId = 3 });
            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });

            // Movie 2: director=Zara, producer=Eve, category=Action
            context.MoviePeople.Add(new MovieNamesPosition { Id = 4, MovieId = 2, NamesId = 4, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 5, MovieId = 2, NamesId = 5, PositionId = 2 });
            context.MovieCategories.Add(new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 });

            context.SaveChanges();
        }

        [Fact]
        public void GetMovies_ReturnsAllMovies_WhenNoSearchOrSort()
        {
            using var context = CreateContext(nameof(GetMovies_ReturnsAllMovies_WhenNoSearchOrSort));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetMovies_ReturnsEmpty_WhenNoMoviesExist()
        {
            using var context = CreateContext(nameof(GetMovies_ReturnsEmpty_WhenNoMoviesExist));
            var service = CreateService(context);

            var result = service.GetMovies(null, null);

            Assert.Empty(result);
        }

        [Fact]
        public void GetMovies_FiltersMovies_WhenSearchProvided()
        {
            using var context = CreateContext(nameof(GetMovies_FiltersMovies_WhenSearchProvided));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies("Beta", null);

            Assert.Single(result);
            Assert.Equal("Beta Movie", result[0].Title);
        }

        [Fact]
        public void GetMovies_ReturnsEmpty_WhenSearchMatchesNothing()
        {
            using var context = CreateContext(nameof(GetMovies_ReturnsEmpty_WhenSearchMatchesNothing));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies("NonExistentTerm", null);

            Assert.Empty(result);
        }

        [Fact]
        public void GetMovies_SortsByIdByDefault()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByIdByDefault));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);

            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public void GetMovies_SortsByTitle()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByTitle));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "title");

            Assert.Equal("Alpha Movie", result[0].Title);
            Assert.Equal("Beta Movie", result[1].Title);
        }

        [Fact]
        public void GetMovies_SortsByTitleDescending()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByTitleDescending));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "title", desc: true);

            Assert.Equal("Beta Movie", result[0].Title);
            Assert.Equal("Alpha Movie", result[1].Title);
        }

        [Fact]
        public void GetMovies_SortsByDirector()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByDirector));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "director");

            // Alice Director < Zara Director
            Assert.Equal("Beta Movie", result[0].Title);
            Assert.Equal("Alpha Movie", result[1].Title);
        }

        [Fact]
        public void GetMovies_SortsByDirectorDescending()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByDirectorDescending));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "director", desc: true);

            // Zara Director > Alice Director
            Assert.Equal("Alpha Movie", result[0].Title);
            Assert.Equal("Beta Movie", result[1].Title);
        }

        [Fact]
        public void GetMovies_SortsByCategory()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByCategory));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "category");

            // Action < Drama
            Assert.Equal("Alpha Movie", result[0].Title);
            Assert.Equal("Beta Movie", result[1].Title);
        }

        [Fact]
        public void GetMovies_SortsByDefaultId_WhenSortIsUnrecognized()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByDefaultId_WhenSortIsUnrecognized));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, "unknownsort");

            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public void GetMovies_SortsByIdDescending()
        {
            using var context = CreateContext(nameof(GetMovies_SortsByIdDescending));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null, desc: true);

            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
        }

        [Fact]
        public void GetMovies_MapsDirectorsCorrectly()
        {
            using var context = CreateContext(nameof(GetMovies_MapsDirectorsCorrectly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);
            var movie2 = result.First(m => m.Id == 2);

            Assert.Single(movie1.Directors);
            Assert.Contains("Alice Director", movie1.Directors);
            Assert.Single(movie2.Directors);
            Assert.Contains("Zara Director", movie2.Directors);
        }

        [Fact]
        public void GetMovies_MapsActorsCorrectly()
        {
            using var context = CreateContext(nameof(GetMovies_MapsActorsCorrectly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);
            var movie2 = result.First(m => m.Id == 2);

            Assert.Single(movie1.Actors);
            Assert.Contains("Bob Actor", movie1.Actors);
            Assert.Empty(movie2.Actors);
        }

        [Fact]
        public void GetMovies_MapsWritersCorrectly()
        {
            using var context = CreateContext(nameof(GetMovies_MapsWritersCorrectly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);
            var movie2 = result.First(m => m.Id == 2);

            Assert.Single(movie1.Writers);
            Assert.Contains("Charlie Writer", movie1.Writers);
            Assert.Empty(movie2.Writers);
        }

        [Fact]
        public void GetMovies_MapsProducersCorrectly()
        {
            using var context = CreateContext(nameof(GetMovies_MapsProducersCorrectly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);
            var movie2 = result.First(m => m.Id == 2);

            Assert.Empty(movie1.Producers);
            Assert.Single(movie2.Producers);
            Assert.Contains("Eve Producer", movie2.Producers);
        }

        [Fact]
        public void GetMovies_MapsCategoriesCorrectly()
        {
            using var context = CreateContext(nameof(GetMovies_MapsCategoriesCorrectly));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);
            var movie2 = result.First(m => m.Id == 2);

            Assert.Single(movie1.Categories);
            Assert.Contains("Drama", movie1.Categories);
            Assert.Single(movie2.Categories);
            Assert.Contains("Action", movie2.Categories);
        }

        [Fact]
        public void GetMovies_MapsTitleAndDescription()
        {
            using var context = CreateContext(nameof(GetMovies_MapsTitleAndDescription));
            SeedData(context);
            var service = CreateService(context);

            var result = service.GetMovies(null, null);
            var movie1 = result.First(m => m.Id == 1);

            Assert.Equal("Beta Movie", movie1.Title);
            Assert.Equal("A dramatic film", movie1.Description);
        }

        [Fact]
        public void GetMovies_HandlesMovieWithNoPeople()
        {
            using var context = CreateContext(nameof(GetMovies_HandlesMovieWithNoPeople));
            context.Movies.Add(new Movie { Id = 1, Title = "Lonely Movie" });
            context.SaveChanges();
            var service = CreateService(context);

            var result = service.GetMovies(null, null);

            Assert.Single(result);
            Assert.Empty(result[0].Directors);
            Assert.Empty(result[0].Actors);
            Assert.Empty(result[0].Writers);
            Assert.Empty(result[0].Producers);
            Assert.Empty(result[0].Categories);
        }

        [Fact]
        public void GetMovies_CombinesSearchAndSort()
        {
            using var context = CreateContext(nameof(GetMovies_CombinesSearchAndSort));
            SeedData(context);
            var service = CreateService(context);

            // Search for "Movie" (matches both), sort by title
            var result = service.GetMovies("Movie", "title");

            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha Movie", result[0].Title);
            Assert.Equal("Beta Movie", result[1].Title);
        }
    }
}
