using MovieMVC.DTOs;

namespace MovieMVC.Services
{
    public interface IHomeService
    {
        List<MovieDto> GetMovies(string? search, string? sort, bool desc = false);
    }
}
