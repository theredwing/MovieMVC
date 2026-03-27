using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public interface IHomeRepository
    {
        IQueryable<Movie> GetAllWithIncludes(string? search);
    }
}
