using MovieMVC.Models;

namespace MovieMVC.Repositories
{
    public interface IGraphRepository
    {
        List<NamesLU> GetNamesByPosition(int positionId);
        Dictionary<int, int> GetMovieCountsByCategories(int[] categoryIds);
        Dictionary<int, int> GetMovieCountsByPeople(int[] nameIds, int positionId);
    }
}
