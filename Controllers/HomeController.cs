using Microsoft.AspNetCore.Mvc;
using MovieMVC.Models;
using MovieMVC.Models.ViewModels;
using MovieMVC.Services;
using System.Diagnostics;

namespace MovieMVC.Controllers
{
    public class HomeController: Controller
    {
        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        public IActionResult Index (string? search, string? sort, bool desc = false)
        {
            try
            {
                var dtos = _homeService.GetMovies(search, sort, desc);
                var viewModels = dtos.Select(d => new MovieCreateViewModel
                {
                    Movie = new Movie { Id = d.Id, Title = d.Title ?? "" },
                    MovieDto = d,
                    Title = d.Title ?? ""
                }).ToList();
                ViewData["Title"] = "Home Page";
                var indexVm = new MovieIndexViewModel
                {
                    Movies = viewModels,
                    CurrentSort = sort,
                    NextSort = !desc,
                    Search = search
                };
                return View(indexVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movie index");
                TempData["ErrorMessage"] = $"An error occurred loading movies: {ex.Message}";
                return View(new MovieIndexViewModel());
            }
        }

        public IActionResult Privacy ()
        {
            return View ();
        }

        [ResponseCache (Duration = 0,Location = ResponseCacheLocation.None,NoStore = true)]
        public IActionResult Error ()
        {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
