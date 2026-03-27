using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly AppDbContext _context;

        public HomeRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Movie> GetAllWithIncludes(string? search)
        {
            var query = _context.Movies
                .Include(m => m.MoviePeople).ThenInclude(mp => mp.Name)
                .Include(m => m.MovieCategory).ThenInclude(mc => mc.Category)
                .AsQueryable();

            if (search == null)
            {
                return query;
            }
            else
            {    
                return query.Where(m => m.Title.Contains(search) || m.Description!.Contains (search) || m.MovieCategory.Any (ma => ma.Category!.Category.Contains (search)) || m.MoviePeople.Any (ma => ma.Name!.Name.Contains (search)) ||
                      m.MoviePeople.Any (ma => ma.Position!.Position.Contains (search))); 
            }
        }
    }
}
