using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public interface ILookupRepository
    {
        int GetPositionId(string positionName);
        Task<int> GetPositionIdAsync(string positionName);
        Dictionary<string, int> GetAllPositionIds();
        Task<Dictionary<string, int>> GetAllPositionIdsAsync();
        List<NamesLU> GetAllNames();
        List<NamesLU> GetAllNamesWithMovieCount();
        List<CategoryLU> GetAllCategories();
    }
}
