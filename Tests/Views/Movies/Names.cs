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
    public class Names
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MoviesNames_{dbName}")
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

        private void SeedNames(AppDbContext context)
        {
            context.Actors.AddRange(
                new NamesLU { Id = 1, Name = "Charlie" },
                new NamesLU { Id = 2, Name = "Alice" },
                new NamesLU { Id = 3, Name = "Bob" });
            context.SaveChanges();
        }

        [Fact]
        public void Names_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(Names_ReturnsViewResult));
            SeedNames(context);
            var controller = CreateController(context);

            var result = controller.Names(null, null, null);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Names_ReturnsNamesListModel()
        {
            using var context = CreateContext(nameof(Names_ReturnsNamesListModel));
            SeedNames(context);
            var controller = CreateController(context);

            var result = controller.Names(null, null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<List<NamesLU>>(result.Model);
        }

        [Fact]
        public void Names_ListContainsAllNames()
        {
            using var context = CreateContext(nameof(Names_ListContainsAllNames));
            SeedNames(context);
            var controller = CreateController(context);

            var result = controller.Names(null, null, null) as ViewResult;
            var model = result!.Model as List<NamesLU>;

            Assert.Equal(3, model!.Count);
        }

        [Fact]
        public void Names_ListIsOrderedByName()
        {
            using var context = CreateContext(nameof(Names_ListIsOrderedByName));
            SeedNames(context);
            var controller = CreateController(context);

            var result = controller.Names(null, null, null) as ViewResult;
            var model = result!.Model as List<NamesLU>;

            Assert.Equal("Alice", model![0].Name);
            Assert.Equal("Bob", model[1].Name);
            Assert.Equal("Charlie", model[2].Name);
        }

        [Fact]
        public async Task AddName_RedirectsToNames()
        {
            using var context = CreateContext(nameof(AddName_RedirectsToNames));
            var controller = CreateController(context);

            var result = await controller.AddName("New Name", null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Names", redirect.ActionName);
        }

        [Fact]
        public async Task AddName_AddsNameToDatabase()
        {
            using var context = CreateContext(nameof(AddName_AddsNameToDatabase));
            var controller = CreateController(context);

            await controller.AddName("New Name", null, null, null);

            Assert.Single(context.Actors);
            Assert.Equal("New Name", context.Actors.First().Name);
        }

        [Fact]
        public async Task AddName_SetsSuccessMessage()
        {
            using var context = CreateContext(nameof(AddName_SetsSuccessMessage));
            var controller = CreateController(context);

            await controller.AddName("New Name", null, null, null);

            Assert.Contains("added successfully", controller.TempData["SuccessMessage"] as string);
        }

        [Fact]
        public async Task AddName_ShowsError_WhenDuplicate()
        {
            using var context = CreateContext(nameof(AddName_ShowsError_WhenDuplicate));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.AddName("Alice", null, null, null);

            Assert.Contains("already exists", controller.TempData["ErrorMessage"] as string);
        }

        [Fact]
        public async Task AddName_TrimsWhitespace()
        {
            using var context = CreateContext(nameof(AddName_TrimsWhitespace));
            var controller = CreateController(context);

            await controller.AddName("  Trimmed Name  ", null, null, null);

            Assert.Equal("Trimmed Name", context.Actors.First().Name);
        }

        [Fact]
        public async Task EditName_UpdatesName()
        {
            using var context = CreateContext(nameof(EditName_UpdatesName));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.EditName(1, "Charlie Updated", null, null, null);

            var name = await context.Actors.FindAsync(1);
            Assert.Equal("Charlie Updated", name!.Name);
        }

        [Fact]
        public async Task EditName_RedirectsToNames()
        {
            using var context = CreateContext(nameof(EditName_RedirectsToNames));
            SeedNames(context);
            var controller = CreateController(context);

            var result = await controller.EditName(1, "Charlie Updated", null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Names", redirect.ActionName);
        }

        [Fact]
        public async Task EditName_ShowsError_WhenEmpty()
        {
            using var context = CreateContext(nameof(EditName_ShowsError_WhenEmpty));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.EditName(1, "  ", null, null, null);

            Assert.Contains("cannot be empty", controller.TempData["ErrorMessage"] as string);
        }

        [Fact]
        public async Task EditName_ShowsError_WhenDuplicate()
        {
            using var context = CreateContext(nameof(EditName_ShowsError_WhenDuplicate));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.EditName(1, "Alice", null, null, null);

            Assert.Contains("already exists", controller.TempData["ErrorMessage"] as string);
        }

        [Fact]
        public async Task EditName_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateContext(nameof(EditName_ReturnsNotFound_WhenMissing));
            SeedNames(context);
            var controller = CreateController(context);

            var result = await controller.EditName(999, "New Name", null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteName_RedirectsToNames()
        {
            using var context = CreateContext(nameof(DeleteName_RedirectsToNames));
            SeedNames(context);
            var controller = CreateController(context);

            var result = await controller.DeleteName(1, null, null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Names", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteName_RemovesNameFromDatabase()
        {
            using var context = CreateContext(nameof(DeleteName_RemovesNameFromDatabase));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.DeleteName(1, null, null, null);

            Assert.Equal(2, context.Actors.Count());
            Assert.Null(await context.Actors.FindAsync(1));
        }

        [Fact]
        public async Task DeleteName_SetsSuccessMessage()
        {
            using var context = CreateContext(nameof(DeleteName_SetsSuccessMessage));
            SeedNames(context);
            var controller = CreateController(context);

            await controller.DeleteName(1, null, null, null);

            Assert.Contains("deleted successfully", controller.TempData["SuccessMessage"] as string);
        }

        [Fact]
        public async Task DeleteName_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateContext(nameof(DeleteName_ReturnsNotFound_WhenMissing));
            SeedNames(context);
            var controller = CreateController(context);

            var result = await controller.DeleteName(999, null, null, null);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
