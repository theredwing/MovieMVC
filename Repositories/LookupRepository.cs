using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly AppDbContext _context;

        public LookupRepository(AppDbContext context)
        {
            _context = context;
        }

        public int GetPositionId(string positionName)
        {
            return _context.Positions
                .AsNoTracking()
                .Where(p => p.Position.ToLower() == positionName)
                .Select(p => p.Id)
                .FirstOrDefault();
        }

        public async Task<int> GetPositionIdAsync(string positionName)
        {
            return await _context.Positions
                .AsNoTracking()
                .Where(p => p.Position.ToLower() == positionName)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();
        }

        public Dictionary<string, int> GetAllPositionIds()
        {
            return _context.Positions
                .AsNoTracking()
                .ToDictionary(p => p.Position.ToLower(), p => p.Id);
        }

        public async Task<Dictionary<string, int>> GetAllPositionIdsAsync()
        {
            return await _context.Positions
                .AsNoTracking()
                .ToDictionaryAsync(p => p.Position.ToLower(), p => p.Id);
        }

        public List<NamesLU> GetAllNames()
        {
            return _context.Actors.AsNoTracking().OrderBy(n => n.Name).ToList();
        }

        public List<NamesLU> GetAllNamesWithMovieCount()
        {
            return _context.Actors.AsNoTracking().Include(n => n.MoviePeople).OrderBy(n => n.Name).ToList();
        }

        public List<CategoryLU> GetAllCategories()
        {
            return _context.Categories.AsNoTracking().OrderBy(c => c.Category).ToList();
        }
    }
}
