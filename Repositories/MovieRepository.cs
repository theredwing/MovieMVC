using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Movie?> GetWithDetailsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MoviePeople).ThenInclude(mp => mp.Name)
                .Include(m => m.MoviePeople).ThenInclude(mp => mp.Position)
                .Include(m => m.MovieCategory).ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Movie?> GetWithRelationsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MoviePeople)
                .Include(m => m.MovieCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Movie?> FindAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public void Add(Movie movie)
        {
            _context.Movies.Add(movie);
        }

        public void Remove(Movie movie)
        {
            _context.Movies.Remove(movie);
        }

        public async Task<int> GetPositionIdAsync(string positionName)
        {
            return await _context.Positions
                .Where(p => p.Position.ToLower() == positionName)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();
        }

        public void AddPerson(MovieNamesPosition person)
        {
            _context.MoviePeople.Add(person);
        }

        public void RemovePeople(IEnumerable<MovieNamesPosition> people)
        {
            _context.MoviePeople.RemoveRange(people);
        }

        public void AddCategory(MovieCategory category)
        {
            _context.MovieCategories.Add(category);
        }

        public void RemoveCategories(IEnumerable<MovieCategory> categories)
        {
            _context.MovieCategories.RemoveRange(categories);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public List<NamesLU> GetAllNames()
        {
            return _context.Actors.OrderBy(n => n.Name).ToList();
        }

        public List<CategoryLU> GetAllCategories()
        {
            return _context.Categories.OrderBy(c => c.Category).ToList();
        }
    }
}
