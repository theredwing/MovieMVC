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

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public IActionResult Index (string? search, string? sort, bool desc = false)
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
