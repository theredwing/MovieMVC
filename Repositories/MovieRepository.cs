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
                .AsNoTracking()
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

        public async Task MergeNamesAsync(int targetId, List<int> sourceIds)
        {
            foreach (var sourceId in sourceIds)
            {
                var sourceRecords = await _context.MoviePeople
                    .Where(mp => mp.NamesId == sourceId)
                    .ToListAsync();

                foreach (var record in sourceRecords)
                {
                    var duplicate = await _context.MoviePeople
                        .AnyAsync(mp => mp.MovieId == record.MovieId
                            && mp.NamesId == targetId
                            && mp.PositionId == record.PositionId);

                    if (duplicate)
                    {
                        _context.MoviePeople.Remove(record);
                    }
                    else
                    {
                        record.NamesId = targetId;
                    }
                }

                var sourceName = await _context.Actors.FindAsync(sourceId);
                if (sourceName != null)
                {
                    _context.Actors.Remove(sourceName);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<NamesLU?> GetNameByIdAsync(int id)
        {
            return await _context.Actors.FindAsync(id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Actors
                .AnyAsync(n => n.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || n.Id != excludeId.Value));
        }

        public async Task AddNameAsync(NamesLU name)
        {
            _context.Actors.Add(name);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNameAsync(NamesLU name)
        {
            _context.Actors.Update(name);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteNameAsync(int id)
        {
            var name = await _context.Actors
                .Include(n => n.MoviePeople)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (name == null) return false;

            if (name.MoviePeople != null && name.MoviePeople.Count > 0)
                _context.MoviePeople.RemoveRange(name.MoviePeople);

            _context.Actors.Remove(name);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
