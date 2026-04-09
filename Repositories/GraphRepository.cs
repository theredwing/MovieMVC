using MovieMVC.Data;
using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public class GraphRepository : IGraphRepository
    {
        private readonly AppDbContext _context;

        public GraphRepository(AppDbContext context)
        {
            _context = context;
        }

        public int GetPositionId(string positionName)
        {
            return _context.Positions
                .Where(p => p.Position.ToLower() == positionName)
                .Select(p => p.Id)
                .FirstOrDefault();
        }

        public List<CategoryLU> GetAllCategories()
        {
            return _context.Categories.OrderBy(c => c.Category).ToList();
        }

        public List<NamesLU> GetNamesByPosition(int positionId)
        {
            var nameIds = _context.MoviePeople
                .Where(mp => mp.PositionId == positionId)
                .Select(mp => mp.NamesId)
                .Distinct()
                .ToList();

            return _context.Actors
                .Where(n => nameIds.Contains(n.Id))
                .OrderBy(n => n.Name)
                .ToList();
        }

        public Dictionary<int, int> GetMovieCountsByCategories(int[] categoryIds)
        {
            var rows = _context.MovieCategories
                .Where(mc => categoryIds.Contains(mc.CategoryId))
                .Select(mc => new { mc.CategoryId, mc.MovieId })
                .ToList();

            return rows
                .GroupBy(r => r.CategoryId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.MovieId).Distinct().Count());
        }

        public Dictionary<int, int> GetMovieCountsByPeople(int[] nameIds, int positionId)
        {
            var rows = _context.MoviePeople
                .Where(mp => nameIds.Contains(mp.NamesId) && mp.PositionId == positionId)
                .Select(mp => new { mp.NamesId, mp.MovieId, mp.PositionId, mp.Movie })
                .ToList();

            return rows
                .GroupBy(r => r.NamesId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.MovieId).Distinct().Count());
        }
    }
}
