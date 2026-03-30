using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public interface IMovieRepository
    {
        Task<Movie?> GetWithDetailsAsync(int id);
        Task<Movie?> GetWithRelationsAsync(int id);
        Task<Movie?> FindAsync(int id);
        void Add(Movie movie);
        void Remove(Movie movie);
        Task<int> GetPositionIdAsync(string positionName);
        void AddPerson(MovieNamesPosition person);
        void RemovePeople(IEnumerable<MovieNamesPosition> people);
        void AddCategory(MovieCategory category);
        void RemoveCategories(IEnumerable<MovieCategory> categories);
        Task SaveChangesAsync();
        Task MergeNamesAsync(int targetId, List<int> sourceIds);
        List<NamesLU> GetAllNames();
        List<CategoryLU> GetAllCategories();
        Task<NamesLU?> GetNameByIdAsync(int id);
        Task AddNameAsync(NamesLU name);
        Task UpdateNameAsync(NamesLU name);
        Task<bool> DeleteNameAsync(int id);
    }
}
