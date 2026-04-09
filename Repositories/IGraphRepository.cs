using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public interface IGraphRepository
    {
        int GetPositionId(string positionName);
        List<CategoryLU> GetAllCategories();
        List<NamesLU> GetNamesByPosition(int positionId);
        Dictionary<int, int> GetMovieCountsByCategories(int[] categoryIds);
        Dictionary<int, int> GetMovieCountsByPeople(int[] nameIds, int positionId);
    }
}
