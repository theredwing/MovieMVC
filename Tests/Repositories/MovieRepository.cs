using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;
using Xunit;

namespace Tests.Repositories
{
    public class MovieRepository
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MovieRepo_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private void SeedBasicData(AppDbContext context)
        {
            var position = new PositionLU { Id = 1, Position = "Actor" };
            var category = new CategoryLU { Id = 1, Category = "Drama" };
            var name = new NamesLU { Id = 1, Name = "John Doe" };
            var movie = new Movie { Id = 1, Title = "Test Movie", Description = "A test movie" };

            context.Positions.Add(position);
            context.Categories.Add(category);
            context.Actors.Add(name);
            context.Movies.Add(movie);

            context.MoviePeople.Add(new MovieNamesPosition
            {
                Id = 1,
                MovieId = 1,
                NamesId = 1,
                PositionId = 1
            });

            context.MovieCategories.Add(new MovieCategory
            {
                Id = 1,
                MovieId = 1,
                CategoryId = 1
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task GetWithDetailsAsync_ReturnsMovieWithIncludes()
        {
            using var context = CreateContext(nameof(GetWithDetailsAsync_ReturnsMovieWithIncludes));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.GetWithDetailsAsync(1);

            Assert.NotNull(movie);
            Assert.Equal("Test Movie", movie.Title);
            Assert.NotNull(movie.MoviePeople);
            Assert.Single(movie.MoviePeople);
            Assert.Equal("John Doe", movie.MoviePeople[0].Name.Name);
            Assert.Equal("Actor", movie.MoviePeople[0].Position.Position);
            Assert.NotNull(movie.MovieCategory);
            Assert.Single(movie.MovieCategory);
            Assert.Equal("Drama", movie.MovieCategory[0].Category.Category);
        }

        [Fact]
        public async Task GetWithDetailsAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetWithDetailsAsync_ReturnsNull_WhenNotFound));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.GetWithDetailsAsync(999);

            Assert.Null(movie);
        }

        [Fact]
        public async Task GetWithRelationsAsync_ReturnsMovieWithRelations()
        {
            using var context = CreateContext(nameof(GetWithRelationsAsync_ReturnsMovieWithRelations));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.GetWithRelationsAsync(1);

            Assert.NotNull(movie);
            Assert.NotNull(movie.MoviePeople);
            Assert.Single(movie.MoviePeople);
            Assert.NotNull(movie.MovieCategory);
            Assert.Single(movie.MovieCategory);
        }

        [Fact]
        public async Task GetWithRelationsAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetWithRelationsAsync_ReturnsNull_WhenNotFound));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.GetWithRelationsAsync(999);

            Assert.Null(movie);
        }

        [Fact]
        public async Task FindAsync_ReturnsMovie_WhenExists()
        {
            using var context = CreateContext(nameof(FindAsync_ReturnsMovie_WhenExists));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.FindAsync(1);

            Assert.NotNull(movie);
            Assert.Equal("Test Movie", movie.Title);
        }

        [Fact]
        public async Task FindAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(FindAsync_ReturnsNull_WhenNotFound));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var movie = await repo.FindAsync(999);

            Assert.Null(movie);
        }

        [Fact]
        public async Task Add_AddsMovieToContext()
        {
            using var context = CreateContext(nameof(Add_AddsMovieToContext));
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var movie = new Movie { Id = 10, Title = "New Movie" };

            repo.Add(movie);
            await context.SaveChangesAsync();

            Assert.Equal(1, await context.Movies.CountAsync());
            Assert.Equal("New Movie", (await context.Movies.FirstAsync()).Title);
        }

        [Fact]
        public async Task Remove_RemovesMovieFromContext()
        {
            using var context = CreateContext(nameof(Remove_RemovesMovieFromContext));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var movie = await context.Movies.FirstAsync();

            repo.Remove(movie);
            await context.SaveChangesAsync();

            Assert.Equal(0, await context.Movies.CountAsync());
        }

        [Fact]
        public async Task AddPerson_AddsMovieNamesPosition()
        {
            using var context = CreateContext(nameof(AddPerson_AddsMovieNamesPosition));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var person = new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 1, PositionId = 1 };

            repo.AddPerson(person);
            await context.SaveChangesAsync();

            Assert.Equal(2, await context.MoviePeople.CountAsync());
        }

        [Fact]
        public async Task RemovePeople_RemovesMultipleEntries()
        {
            using var context = CreateContext(nameof(RemovePeople_RemovesMultipleEntries));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var people = await context.MoviePeople.ToListAsync();

            repo.RemovePeople(people);
            await context.SaveChangesAsync();

            Assert.Equal(0, await context.MoviePeople.CountAsync());
        }

        [Fact]
        public async Task AddCategory_AddsMovieCategory()
        {
            using var context = CreateContext(nameof(AddCategory_AddsMovieCategory));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var category = new MovieCategory { Id = 2, MovieId = 1, CategoryId = 1 };

            repo.AddCategory(category);
            await context.SaveChangesAsync();

            Assert.Equal(2, await context.MovieCategories.CountAsync());
        }

        [Fact]
        public async Task RemoveCategories_RemovesMultipleEntries()
        {
            using var context = CreateContext(nameof(RemoveCategories_RemovesMultipleEntries));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var categories = await context.MovieCategories.ToListAsync();

            repo.RemoveCategories(categories);
            await context.SaveChangesAsync();

            Assert.Equal(0, await context.MovieCategories.CountAsync());
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsChanges()
        {
            using var context = CreateContext(nameof(SaveChangesAsync_PersistsChanges));
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            context.Movies.Add(new Movie { Id = 5, Title = "Pending Movie" });

            await repo.SaveChangesAsync();

            Assert.Equal(1, await context.Movies.CountAsync());
        }

        [Fact]
        public async Task GetNameByIdAsync_ReturnsName_WhenExists()
        {
            using var context = CreateContext(nameof(GetNameByIdAsync_ReturnsName_WhenExists));
            context.Actors.Add(new NamesLU { Id = 1, Name = "Jane" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var name = await repo.GetNameByIdAsync(1);

            Assert.NotNull(name);
            Assert.Equal("Jane", name.Name);
        }

        [Fact]
        public async Task GetNameByIdAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext(nameof(GetNameByIdAsync_ReturnsNull_WhenNotFound));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var name = await repo.GetNameByIdAsync(999);

            Assert.Null(name);
        }

        [Fact]
        public async Task NameExistsAsync_ReturnsTrue_WhenNameExists()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ReturnsTrue_WhenNameExists));
            context.Actors.Add(new NamesLU { Id = 1, Name = "John" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var exists = await repo.NameExistsAsync("john");

            Assert.True(exists);
        }

        [Fact]
        public async Task NameExistsAsync_ReturnsFalse_WhenNameDoesNotExist()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ReturnsFalse_WhenNameDoesNotExist));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var exists = await repo.NameExistsAsync("NonExistent");

            Assert.False(exists);
        }

        [Fact]
        public async Task NameExistsAsync_ExcludesId_WhenProvided()
        {
            using var context = CreateContext(nameof(NameExistsAsync_ExcludesId_WhenProvided));
            context.Actors.Add(new NamesLU { Id = 4, Name = "John" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var exists = await repo.NameExistsAsync("john", excludeId: 4);

            Assert.False(exists);
        }

        [Fact]
        public async Task AddNameAsync_AddsAndSaves()
        {
            using var context = CreateContext(nameof(AddNameAsync_AddsAndSaves));
            var repo = new MovieMVC.Repositories.MovieRepository(context);
            var name = new NamesLU { Id = 2, Name = "NewName" };

            await repo.AddNameAsync(name);

            Assert.Equal(1, await context.Actors.CountAsync());
            Assert.Equal("NewName", (await context.Actors.FirstAsync()).Name);
        }

        [Fact]
        public async Task UpdateNameAsync_UpdatesAndSaves()
        {
            using var context = CreateContext(nameof(UpdateNameAsync_UpdatesAndSaves));
            context.Actors.Add(new NamesLU { Id = 1, Name = "OldName" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var name = await context.Actors.FindAsync(1);
            name!.Name = "UpdatedName";
            await repo.UpdateNameAsync(name);

            var updated = await context.Actors.FindAsync(1);
            Assert.Equal("UpdatedName", updated!.Name);
        }

        [Fact]
        public async Task DeleteNameAsync_ReturnsTrue_AndRemovesName()
        {
            using var context = CreateContext(nameof(DeleteNameAsync_ReturnsTrue_AndRemovesName));
            context.Actors.Add(new NamesLU { Id = 1, Name = "ToDelete" });
            context.SaveChanges();
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var result = await repo.DeleteNameAsync(1);

            Assert.True(result);
            Assert.Equal(0, await context.Actors.CountAsync());
        }

        [Fact]
        public async Task DeleteNameAsync_ReturnsFalse_WhenNotFound()
        {
            using var context = CreateContext(nameof(DeleteNameAsync_ReturnsFalse_WhenNotFound));
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var result = await repo.DeleteNameAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteNameAsync_RemovesAssociatedMoviePeople()
        {
            using var context = CreateContext(nameof(DeleteNameAsync_RemovesAssociatedMoviePeople));
            SeedBasicData(context);
            var repo = new MovieMVC.Repositories.MovieRepository(context);

            var result = await repo.DeleteNameAsync(1);

            Assert.True(result);
            Assert.Equal(0, await context.MoviePeople.CountAsync());
            Assert.Equal(0, await context.Actors.CountAsync());
        }

        [Fact]
        public async Task MergeNamesAsync_MergesSourceIntoTarget()
        {
            using var context = CreateContext(nameof(MergeNamesAsync_MergesSourceIntoTarget));

            var position = new PositionLU { Id = 1, Position = "Actor" };
            var target = new NamesLU { Id = 1, Name = "Target" };
            var source = new NamesLU { Id = 2, Name = "Source" };
            var movie = new Movie { Id = 1, Title = "Movie1" };

            context.Positions.Add(position);
            context.Actors.AddRange(target, source);
            context.Movies.Add(movie);
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 2, PositionId = 1 });
            context.SaveChanges();

            var repo = new MovieMVC.Repositories.MovieRepository(context);
            await repo.MergeNamesAsync(1, [2]);

            // Source name should be deleted
            Assert.Null(await context.Actors.FindAsync(2));
            // The record should now point to target
            var people = await context.MoviePeople.ToListAsync();
            Assert.Single(people);
            Assert.Equal(1, people[0].NamesId);
        }

        [Fact]
        public async Task MergeNamesAsync_RemovesDuplicates()
        {
            using var context = CreateContext(nameof(MergeNamesAsync_RemovesDuplicates));

            var position = new PositionLU { Id = 1, Position = "Actor" };
            var target = new NamesLU { Id = 1, Name = "Target" };
            var source = new NamesLU { Id = 2, Name = "Source" };
            var movie = new Movie { Id = 1, Title = "Movie1" };

            context.Positions.Add(position);
            context.Actors.AddRange(target, source);
            context.Movies.Add(movie);
            // Both target and source have the same movie+position — duplicate scenario
            context.MoviePeople.Add(new MovieNamesPosition { Id = 1, MovieId = 1, NamesId = 1, PositionId = 1 });
            context.MoviePeople.Add(new MovieNamesPosition { Id = 2, MovieId = 1, NamesId = 2, PositionId = 1 });
            context.SaveChanges();

            var repo = new MovieMVC.Repositories.MovieRepository(context);
            await repo.MergeNamesAsync(1, [2]);

            // Source name deleted
            Assert.Null(await context.Actors.FindAsync(2));
            // Duplicate removed, only target's record remains
            var people = await context.MoviePeople.ToListAsync();
            Assert.Single(people);
            Assert.Equal(1, people[0].NamesId);
        }
    }
}
