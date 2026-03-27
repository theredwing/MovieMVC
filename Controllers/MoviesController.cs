using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieMVC.Models;
using MovieMVC.Models.ViewModels;
using MovieMVC.Services;

namespace MovieMVC.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    public IActionResult Create(string? sort, bool? desc, string? search)
    {
        return View(BuildFormViewModel(new Movie()));
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

        await _movieService.CreateMovieAsync(movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories);
        return RedirectToAction(nameof(Details), new { id = movie.Id, sort, desc, search });
    }

    public async Task<IActionResult> Edit(int id, string? sort, bool? desc, string? search)
    {
        var movie = await _movieService.GetMovieDetailsAsync(id);
        if (movie == null) return NotFound();

        var selections = await _movieService.GetSelectedIdsAsync(movie);
        return View(BuildFormViewModel(movie, selections.DirectorIds, selections.ProducerIds, selections.WriterIds, selections.ActorIds, selections.CategoryIds));
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

        var result = await _movieService.UpdateMovieAsync(id, movie, selectedDirectors, selectedProducers, selectedWriters, selectedActors, selectedCategories);
        if (result == null) return NotFound();

        return RedirectToAction(nameof(Details), new { id, sort, desc, search });
    }

    public async Task<IActionResult> Details(int id, string? sort, bool? desc, string? search)
    {
        var movie = await _movieService.GetMovieDetailsAsync(id);
        if (movie == null) return NotFound();

        return View(movie);
    }

    public async Task<IActionResult> Delete(int id, string? sort, bool? desc, string? search)
    {
        var movie = await _movieService.GetMovieDetailsAsync(id);
        if (movie == null) return NotFound();
        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string? sort, bool? desc, string? search)
    {
        var deleted = await _movieService.DeleteMovieAsync(id);
        if (!deleted) return NotFound();

        return RedirectToAction("Index", "Home", new { sort, desc, search });
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
