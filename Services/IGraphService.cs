using MovieMVC.Models.ViewModels;

namespace MovieMVC.Services
{
    public interface IGraphService
    {
        GraphViewModel BuildViewModel(string? selectedType, int[]? selectedIds, string? sort, bool? desc, string? search);
    }
}
