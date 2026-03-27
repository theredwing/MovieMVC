using MovieMVC.DTOs;
using MovieMVC.Models;

namespace MovieMVC.Services
{
    public interface IMovieService
    {
        Task<Movie?> GetMovieDetailsAsync(int id);
        Task<MovieSelections> GetSelectedIdsAsync(Movie movie);
        Task CreateMovieAsync(Movie movie, int[]? selectedDirectors, int[]? selectedProducers, int[]? selectedWriters, int[]? selectedActors, int[]? selectedCategories);
        Task<Movie?> UpdateMovieAsync(int id, Movie updatedMovie, int[]? selectedDirectors, int[]? selectedProducers, int[]? selectedWriters, int[]? selectedActors, int[]? selectedCategories);
        Task<bool> DeleteMovieAsync(int id);
        List<NamesLU> GetAllNames();
        List<CategoryLU> GetAllCategories();
    }
}
