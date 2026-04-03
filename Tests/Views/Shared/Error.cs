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

namespace Tests.Views.Shared
{
    public class Error
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"SharedError_{dbName}")
                .Options;
            return new AppDbContext(options);
        }

        private HomeController CreateController(AppDbContext context)
        {
            var repo = new HomeRepository(context);
            var service = new HomeService(repo);
            var controller = new HomeController(service, NullLogger<HomeController>.Instance);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new TestTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public void Error_ReturnsViewResult()
        {
            using var context = CreateContext(nameof(Error_ReturnsViewResult));
            var controller = CreateController(context);

            var result = controller.Error();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsErrorViewModel()
        {
            using var context = CreateContext(nameof(Error_ReturnsErrorViewModel));
            var controller = CreateController(context);

            var result = controller.Error() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ModelHasRequestId()
        {
            using var context = CreateContext(nameof(Error_ModelHasRequestId));
            var controller = CreateController(context);

            var result = controller.Error() as ViewResult;
            var model = result!.Model as ErrorViewModel;

            Assert.NotNull(model!.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void Error_ShowRequestIdIsTrue_WhenRequestIdIsSet()
        {
            using var context = CreateContext(nameof(Error_ShowRequestIdIsTrue_WhenRequestIdIsSet));
            var controller = CreateController(context);

            var result = controller.Error() as ViewResult;
            var model = result!.Model as ErrorViewModel;

            Assert.True(model!.ShowRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestIdIsFalse_WhenRequestIdIsEmpty()
        {
            var model = new ErrorViewModel { RequestId = "" };

            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestIdIsFalse_WhenRequestIdIsNull()
        {
            var model = new ErrorViewModel { RequestId = null };

            Assert.False(model.ShowRequestId);
        }
    }
}
