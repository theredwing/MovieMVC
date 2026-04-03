using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using Xunit;

namespace Tests.Repositories
{
    public class HomeRepository
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"HomeRepo_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private void SeedData(AppDbContext context)
        {
            var category1 = new CategoryLU { Id = 1, Category = "Drama" };
            var category2 = new CategoryLU { Id = 2, Category = "Action" };
            var name1 = new NamesLU { Id = 1, Name = "John Doe" };
            var name2 = new NamesLU { Id = 2, Name = "Jane Smith" };
            var position = new PositionLU { Id = 1, Position = "Actor" };

            context.Categories.AddRange(category1, category2);
            context.Actors.AddRange(name1, name2);
            context.Positions.Add(position);

            var movie1 = new Movie { Id = 1, Title = "Drama Movie", Description = "A dramatic film" };
            var movie2 = new Movie { Id = 2, Title = "Action Movie", Description = "An action film" };
            context.Movies.AddRange(movie1, movie2);

            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 2, NamesId = 2, PositionId = 1 });

            context.MovieCategories.Add(new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 });
            context.MovieCategories.Add(new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 });

            context.SaveChanges();
        }

        [Fact]
        public void GetAllWithIncludes_ReturnsAllMovies_WhenSearchIsNull()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_ReturnsAllMovies_WhenSearchIsNull));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes(null).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAllWithIncludes_IncludesMoviePeopleAndCategories()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_IncludesMoviePeopleAndCategories));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes(null).ToList();

            foreach (var movie in result)
            {
                Assert.NotNull(movie.MoviePeople);
                Assert.NotEmpty(movie.MoviePeople);
                Assert.NotNull(movie.MovieCategory);
                Assert.NotEmpty(movie.MovieCategory);
            }
        }

        [Fact]
        public void GetAllWithIncludes_FiltersByTitle()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_FiltersByTitle));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("Drama").ToList();

            Assert.Single(result);
            Assert.Equal("Drama Movie", result[0].Title);
        }

        [Fact]
        public void GetAllWithIncludes_FiltersByDescription()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_FiltersByDescription));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("action film").ToList();

            Assert.Single(result);
            Assert.Equal("Action Movie", result[0].Title);
        }

        [Fact]
        public void GetAllWithIncludes_FiltersByCategoryName()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_FiltersByCategoryName));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("Action").ToList();

            Assert.Contains(result, m => m.Title == "Action Movie");
        }

        [Fact]
        public void GetAllWithIncludes_FiltersByPersonName()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_FiltersByPersonName));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("Jane Smith").ToList();

            Assert.Single(result);
            Assert.Equal("Action Movie", result[0].Title);
        }

        [Fact]
        public void GetAllWithIncludes_FiltersByPositionName()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_FiltersByPositionName));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("Actor").ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAllWithIncludes_ReturnsEmpty_WhenNoMatch()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_ReturnsEmpty_WhenNoMatch));
            SeedData(context);
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes("NonExistentSearchTerm").ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAllWithIncludes_ReturnsEmpty_WhenNoMoviesExist()
        {
            using var context = CreateContext(nameof(GetAllWithIncludes_ReturnsEmpty_WhenNoMoviesExist));
            var repo = new MovieMVC.Repositories.HomeRepository(context);

            var result = repo.GetAllWithIncludes(null).ToList();

            Assert.Empty(result);
        }
    }
}
