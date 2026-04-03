using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieMVC.Controllers;
using MovieMVC.Data;
using MovieMVC.Repositories;
using MovieMVC.Services;
using Xunit;

namespace Tests.Views.Home
{
    public class Privacy
    {
        private HomeController CreateController()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"HomePrivacy_{nameof(Privacy)}")
                .Options;
            var context = new AppDbContext(options);
            var repo = new HomeRepository(context);
            var service = new HomeService(repo);
            var controller = new HomeController(service, NullLogger<HomeController>.Instance);
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider);
            return controller;
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var controller = CreateController();

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ViewResultIsNotNull()
        {
            var controller = CreateController();

            var result = controller.Privacy() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy_ModelIsNull()
        {
            var controller = CreateController();

            var result = controller.Privacy() as ViewResult;

            Assert.Null(result!.Model);
        }
    }
}
