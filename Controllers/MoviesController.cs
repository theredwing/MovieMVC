using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieMVC.Models;
using MovieMVC.Models.ViewModels;
using MovieMVC.Services;

namespace MovieMVC.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    public IActionResult Create(string? sort, bool? desc, string? search)
    {
        try
        {
            return View(BuildFormViewModel(new Movie()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create form");
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movie movie,
        int[]? selectedDirectors, int[]? selectedProducers,
        int[]? selectedWriters, int[]? selectedActors,
        int[]? selectedCategories, string? sort, bool? desc, string? search)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildFormViewModel(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories));
        }

        try
        {
            await _movieService.CreateMovieAsync(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories);
            return RedirectToAction(nameof(Details), new { id = movie.Id, sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            TempData["ErrorMessage"] = $"An error occurred creating the movie: {ex.Message}";
            return View(BuildFormViewModel(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories));
        }
    }

    public async Task<IActionResult> Edit(int id, string? sort, bool? desc, string? search)
    {
        try
        {
            var movie = await _movieService.GetMovieDetailsAsync(id);
            if (movie == null) return NotFound();

            var selections = await _movieService.GetSelectedIdsAsync(movie);
            return View(BuildFormViewModel(movie, selections.DirectorIds, selections.ProducerIds, selections.WriterIds, selections.ActorIds, selections.CategoryIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit form for movie {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Movie movie,
        int[]? selectedDirectors, int[]? selectedProducers,
        int[]? selectedWriters, int[]? selectedActors,
        int[]? selectedCategories,
        string? sort, bool? desc, string? search)
    {
        if (id != movie.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(BuildFormViewModel(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories));
        }

        try
        {
            var result = await _movieService.UpdateMovieAsync(id, movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories);
            if (result == null) return NotFound();

            return RedirectToAction(nameof(Details), new { id, sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred updating the movie: {ex.Message}";
            return View(BuildFormViewModel(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories));
        }
    }

    public async Task<IActionResult> Details(int id, string? sort, bool? desc, string? search)
    {
        try
        {
            var movie = await _movieService.GetMovieDetailsAsync(id);
            if (movie == null) return NotFound();

            return View(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Details for movie {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    public async Task<IActionResult> Delete(int id, string? sort, bool? desc, string? search)
    {
        try
        {
            var movie = await _movieService.GetMovieDetailsAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Delete for movie {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string? sort, bool? desc, string? search)
    {
        try
        {
            var deleted = await _movieService.DeleteMovieAsync(id);
            if (!deleted) return NotFound();

            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred deleting the movie: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    public IActionResult MergeNames()
    {
        try
        {
            TempData["SuccessMessage"] = null;
            return View(new MergeNamesViewModel
            {
                Names = new SelectList(_movieService.GetAllNames(), "Id", "Name")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading MergeNames form");
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MergeNames(int targetId, int sourceId2, int? sourceId3,
        int? sourceId4, int? sourceId5, int? sourceId6, string? sort, bool? desc, string? search)
    {
        if (targetId == 0 || sourceId2 == 0)
        {
            ModelState.AddModelError("", "Correct Name and Merge Name 2 are required.");
            return View(new MergeNamesViewModel
            {
                Names = new SelectList(_movieService.GetAllNames(), "Id", "Name")
            });
        }

        var sourceIds = new List<int> { sourceId2 };
        if (sourceId3 is > 0) sourceIds.Add(sourceId3.Value);
        if (sourceId4 is > 0) sourceIds.Add(sourceId4.Value);
        if (sourceId5 is > 0) sourceIds.Add(sourceId5.Value);
        if (sourceId6 is > 0) sourceIds.Add(sourceId6.Value);

        sourceIds = sourceIds.Where(id => id != targetId).Distinct().ToList();

        if (sourceIds.Count == 0)
        {
            ModelState.AddModelError("", "At least one merge name must differ from the correct name.");
            return View(new MergeNamesViewModel
            {
                Names = new SelectList(_movieService.GetAllNames(), "Id", "Name")
            });
        }

        try
        {
            await _movieService.MergeNamesAsync(targetId, sourceIds);
            TempData["SuccessMessage"] = "Names merged successfully";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging names");
            TempData["ErrorMessage"] = $"An error occurred merging names: {ex.Message}";
            return View(new MergeNamesViewModel
            {
                Names = new SelectList(_movieService.GetAllNames(), "Id", "Name")
            });
        }
    }

    public IActionResult Names(string? sort, bool? desc, string? search)
    {
        try
        {
            return View(_movieService.GetAllNamesWithMovieCount());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Names Maintenance");
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index", "Home", new { sort, desc, search });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddName(string name, string? sort, bool? desc, string? search)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (await _movieService.NameExistsAsync(name.Trim()))
                {
                    TempData["ErrorMessage"] = $"'{name.Trim()}' already exists.";
                    return RedirectToAction(nameof(Names), new { sort, desc, search });
                }
                await _movieService.AddNameAsync(new NamesLU { Name = name.Trim() });
                TempData["SuccessMessage"] = $"'{name.Trim()}' added successfully.";
            }
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding name");
            TempData["ErrorMessage"] = $"An error occurred adding the name: {ex.Message}";
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditName(int id, string name, string? sort, bool? desc, string? search)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Name cannot be empty.";
            TempData["FocusNameId"] = id;
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }

        try
        {
            var existing = await _movieService.GetNameByIdAsync(id);
            if (existing == null) return NotFound();

            if (await _movieService.NameExistsAsync(name.Trim(), id))
            {
                TempData["ErrorMessage"] = $"'{name.Trim()}' already exists.";
                TempData["FocusNameId"] = id;
                return RedirectToAction(nameof(Names), new { sort, desc, search });
            }

            existing.Name = name.Trim();
            await _movieService.UpdateNameAsync(existing);
            TempData["SuccessMessage"] = $"Name updated to '{name.Trim()}' successfully.";
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing name {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred updating the name: {ex.Message}";
            TempData["FocusNameId"] = id;
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteName(int id, string? sort, bool? desc, string? search)
    {
        try
        {
            var nameEntry = await _movieService.GetNameByIdAsync(id);
            var deleted = await _movieService.DeleteNameAsync(id);
            if (!deleted) return NotFound();

            TempData["SuccessMessage"] = $"'{nameEntry?.Name}' deleted successfully.";
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting name {Id}", id);
            TempData["ErrorMessage"] = $"An error occurred deleting the name: {ex.Message}";
            return RedirectToAction(nameof(Names), new { sort, desc, search });
        }
    }

    private MovieFormViewModel BuildFormViewModel(Movie movie,
        int[]? selectedDirectorIds = null, int[]? selectedProducerIds = null,
        int[]? selectedWriterIds = null, int[]? selectedActorIds = null,
        int[]? selectedCategoryIds = null)
    {
        var names = _movieService.GetAllNames();
        return new MovieFormViewModel
        {
            Movie = movie,
            Directors = new MultiSelectList(names, "Id", "Name", selectedDirectorIds),
            Producers = new MultiSelectList(names, "Id", "Name", selectedProducerIds),
            Writers = new MultiSelectList(names, "Id", "Name", selectedWriterIds),
            Actors = new MultiSelectList(names, "Id", "Name", selectedActorIds),
            Categories = new MultiSelectList(_movieService.GetAllCategories(), "Id", "Category", selectedCategoryIds)
        };
    }
}
