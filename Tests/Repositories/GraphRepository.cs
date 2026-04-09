using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using Xunit;

namespace Tests.Repositories
{
    public class GraphRepository
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GraphRepo_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private void SeedData(AppDbContext context)
        {
            var director = new PositionLU { Id = 1, Position = "Director" };
            var actor = new PositionLU { Id = 2, Position = "Actor" };
            context.Positions.AddRange(director, actor);

            var cat1 = new CategoryLU { Id = 1, Category = "Action" };
            var cat2 = new CategoryLU { Id = 2, Category = "Drama" };
            var cat3 = new CategoryLU { Id = 3, Category = "Comedy" };
            context.Categories.AddRange(cat1, cat2, cat3);

            var name1 = new NamesLU { Id = 1, Name = "Alice" };
            var name2 = new NamesLU { Id = 2, Name = "Bob" };
            var name3 = new NamesLU { Id = 3, Name = "Charlie" };
            context.Actors.AddRange(name1, name2, name3);

            var movie1 = new Movie { Id = 1, Title = "Movie A", Description = "Desc A" };
            var movie2 = new Movie { Id = 2, Title = "Movie B", Description = "Desc B" };
            var movie3 = new Movie { Id = 3, Title = "Movie C", Description = "Desc C" };
            context.Movies.AddRange(movie1, movie2, movie3);

            // Alice directed Movie A and Movie B
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 2, MovieId = 2, NamesId = 1, PositionId = 1 });

            // Bob directed Movie C, and acted in Movie A
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 3, MovieId = 3, NamesId = 2, PositionId = 1 },
                new MovieNamesPosition { Id = 4, MovieId = 1, NamesId = 2, PositionId = 2 });

            // Charlie acted in Movie B and Movie C
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 5, MovieId = 2, NamesId = 3, PositionId = 2 },
                new MovieNamesPosition { Id = 6, MovieId = 3, NamesId = 3, PositionId = 2 });

            // Movie A => Action, Movie B => Drama, Movie C => Action + Drama
            context.MovieCategories.AddRange(
                new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 },
                new MovieCategory { Id = 2, MovieId = 2, CategoryId = 2 },
                new MovieCategory { Id = 3, MovieId = 3, CategoryId = 1 },
                new MovieCategory { Id = 4, MovieId = 3, CategoryId = 2 });

            context.SaveChanges();
        }

        // ── GetPositionId ──

        [Fact]
        public void GetPositionId_ReturnsId_WhenPositionExists()
        {
            using var context = CreateContext(nameof(GetPositionId_ReturnsId_WhenPositionExists));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetPositionId("director");

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetPositionId_ReturnsZero_WhenPositionNotFound()
        {
            using var context = CreateContext(nameof(GetPositionId_ReturnsZero_WhenPositionNotFound));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetPositionId("editor");

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetPositionId_IsCaseInsensitive()
        {
            using var context = CreateContext(nameof(GetPositionId_IsCaseInsensitive));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetPositionId("director");

            Assert.Equal(1, result);
        }

        // ── GetAllCategories ──

        [Fact]
        public void GetAllCategories_ReturnsAllCategories()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsAllCategories));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetAllCategories();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAllCategories_ReturnsOrderedByName()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsOrderedByName));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetAllCategories();

            Assert.Equal("Action", result[0].Category);
            Assert.Equal("Comedy", result[1].Category);
            Assert.Equal("Drama", result[2].Category);
        }

        [Fact]
        public void GetAllCategories_ReturnsEmpty_WhenNoCategoriesExist()
        {
            using var context = CreateContext(nameof(GetAllCategories_ReturnsEmpty_WhenNoCategoriesExist));
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetAllCategories();

            Assert.Empty(result);
        }

        // ── GetNamesByPosition ──

        [Fact]
        public void GetNamesByPosition_ReturnsDirectors()
        {
            using var context = CreateContext(nameof(GetNamesByPosition_ReturnsDirectors));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetNamesByPosition(1);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, n => n.Name == "Alice");
            Assert.Contains(result, n => n.Name == "Bob");
        }

        [Fact]
        public void GetNamesByPosition_ReturnsActors()
        {
            using var context = CreateContext(nameof(GetNamesByPosition_ReturnsActors));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetNamesByPosition(2);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, n => n.Name == "Bob");
            Assert.Contains(result, n => n.Name == "Charlie");
        }

        [Fact]
        public void GetNamesByPosition_ReturnsOrderedByName()
        {
            using var context = CreateContext(nameof(GetNamesByPosition_ReturnsOrderedByName));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetNamesByPosition(1);

            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("Bob", result[1].Name);
        }

        [Fact]
        public void GetNamesByPosition_ReturnsEmpty_WhenNoNamesForPosition()
        {
            using var context = CreateContext(nameof(GetNamesByPosition_ReturnsEmpty_WhenNoNamesForPosition));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetNamesByPosition(99);

            Assert.Empty(result);
        }

        [Fact]
        public void GetNamesByPosition_ReturnsDistinctNames()
        {
            using var context = CreateContext(nameof(GetNamesByPosition_ReturnsDistinctNames));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            // Alice directed 2 movies but should appear only once
            var result = repo.GetNamesByPosition(1);

            Assert.Equal(2, result.Count);
        }

        // ── GetMovieCountsByCategories ──

        [Fact]
        public void GetMovieCountsByCategories_ReturnsCorrectCounts()
        {
            using var context = CreateContext(nameof(GetMovieCountsByCategories_ReturnsCorrectCounts));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            // Action: Movie A + Movie C = 2, Drama: Movie B + Movie C = 2
            var result = repo.GetMovieCountsByCategories([1, 2]);

            Assert.Equal(2, result[1]);
            Assert.Equal(2, result[2]);
        }

        [Fact]
        public void GetMovieCountsByCategories_ReturnsEmpty_WhenNoMatchingIds()
        {
            using var context = CreateContext(nameof(GetMovieCountsByCategories_ReturnsEmpty_WhenNoMatchingIds));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetMovieCountsByCategories([99]);

            Assert.Empty(result);
        }

        [Fact]
        public void GetMovieCountsByCategories_ReturnsSubset_WhenPartialIdsProvided()
        {
            using var context = CreateContext(nameof(GetMovieCountsByCategories_ReturnsSubset_WhenPartialIdsProvided));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetMovieCountsByCategories([1]);

            Assert.Single(result);
            Assert.Equal(2, result[1]);
        }

        [Fact]
        public void GetMovieCountsByCategories_CountsDistinctMovies()
        {
            using var context = CreateContext(nameof(GetMovieCountsByCategories_CountsDistinctMovies));
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var cat = new CategoryLU { Id = 1, Category = "Action" };
            context.Categories.Add(cat);
            var movie = new Movie { Id = 1, Title = "M", Description = "D" };
            context.Movies.Add(movie);
            // Duplicate category entries for same movie
            context.MovieCategories.AddRange(
                new MovieCategory { Id = 1, MovieId = 1, CategoryId = 1 },
                new MovieCategory { Id = 2, MovieId = 1, CategoryId = 1 });
            context.SaveChanges();

            var result = repo.GetMovieCountsByCategories([1]);

            Assert.Equal(1, result[1]);
        }

        // ── GetMovieCountsByPeople ──

        [Fact]
        public void GetMovieCountsByPeople_ReturnsCorrectCounts()
        {
            using var context = CreateContext(nameof(GetMovieCountsByPeople_ReturnsCorrectCounts));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            // Alice directed Movie A + Movie B = 2, Bob directed Movie C = 1
            var result = repo.GetMovieCountsByPeople([1, 2], 1);

            Assert.Equal(2, result[1]);
            Assert.Equal(1, result[2]);
        }

        [Fact]
        public void GetMovieCountsByPeople_FiltersbyPositionId()
        {
            using var context = CreateContext(nameof(GetMovieCountsByPeople_FiltersbyPositionId));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            // Bob acted in 1 movie (positionId 2), not his director credits
            var result = repo.GetMovieCountsByPeople([2], 2);

            Assert.Equal(1, result[2]);
        }

        [Fact]
        public void GetMovieCountsByPeople_ReturnsEmpty_WhenNoMatchingIds()
        {
            using var context = CreateContext(nameof(GetMovieCountsByPeople_ReturnsEmpty_WhenNoMatchingIds));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetMovieCountsByPeople([99], 1);

            Assert.Empty(result);
        }

        [Fact]
        public void GetMovieCountsByPeople_ReturnsEmpty_WhenPositionNotFound()
        {
            using var context = CreateContext(nameof(GetMovieCountsByPeople_ReturnsEmpty_WhenPositionNotFound));
            SeedData(context);
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var result = repo.GetMovieCountsByPeople([1, 2], 99);

            Assert.Empty(result);
        }

        [Fact]
        public void GetMovieCountsByPeople_CountsDistinctMovies()
        {
            using var context = CreateContext(nameof(GetMovieCountsByPeople_CountsDistinctMovies));
            var repo = new MovieMVC.Repositories.GraphRepository(context);

            var pos = new PositionLU { Id = 1, Position = "Director" };
            context.Positions.Add(pos);
            var name = new NamesLU { Id = 1, Name = "Alice" };
            context.Actors.Add(name);
            var movie = new Movie { Id = 1, Title = "M", Description = "D" };
            context.Movies.Add(movie);
            // Duplicate people entries for same movie/position
            context.MoviePeople.AddRange(
                new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 },
                new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.SaveChanges();

            var result = repo.GetMovieCountsByPeople([1], 1);

            Assert.Equal(1, result[1]);
        }
    }
}
