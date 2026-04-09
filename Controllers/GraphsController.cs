using Microsoft.AspNetCore.Mvc;
using MovieMVC.Models.ViewModels;
using MovieMVC.Services;

namespace MovieMVC.Controllers
{
    public class GraphsController : Controller
    {
        private readonly IGraphService _graphService;
        private readonly ILogger<GraphsController> _logger;

        public GraphsController(IGraphService graphService, ILogger<GraphsController> logger)
        {
            _graphService = graphService;
            _logger = logger;
        }

        public IActionResult Index(string? type, int[]? ids,string? sort,bool? desc,string? search)
        {
            try
            {
                var vm = _graphService.BuildViewModel(type, ids, sort, desc, search);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading graph data");
                TempData["ErrorMessage"] = $"An error occurred loading graph data: {ex.Message}";
                return View(new GraphViewModel());
            }
        }
    }
}
