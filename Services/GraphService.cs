using Microsoft.AspNetCore.Mvc.Rendering;
using MovieMVC.Models.ViewModels;
using MovieMVC.Repositories;

namespace MovieMVC.Services
{
    public class GraphService : IGraphService
    {
        private readonly IGraphRepository _repository;

        public GraphService(IGraphRepository repository)
        {
            _repository = repository;
        }

        private static string? GetPositionName(string type) => type switch
        {
            "Directors" => "director",
            "Producers" => "producer",
            "Writers" => "writer",
            "Actors" => "actor",
            _ => null
        };

        public GraphViewModel BuildViewModel(string? selectedType, int[]? selectedIds, string? sort, bool? desc, string? search)
        {
            var vm = new GraphViewModel { SelectedType = selectedType, Sort = sort, Desc = desc, Search = search };

            if (string.IsNullOrEmpty(selectedType))
                return vm;

            if (selectedType == "Categories")
            {
                var categories = _repository.GetAllCategories();
                vm.AvailableItems = categories
                    .Select(c => new SelectListItem(c.Category, c.Id.ToString()))
                    .ToList();
            }
            else
            {
                var positionName = GetPositionName(selectedType);
                if (positionName == null) return vm;

                var positionId = _repository.GetPositionId(positionName);
                if (positionId == 0) return vm;

                var names = _repository.GetNamesByPosition(positionId);
                vm.AvailableItems = names
                    .Select(n => new SelectListItem(n.Name, n.Id.ToString()))
                    .ToList();
            }

            if (selectedIds != null && selectedIds.Length > 0)
            {
                var ids = selectedIds.Where(id => id > 0).Distinct().Take(4).ToArray();
                vm.SelectedIds = ids;

                Dictionary<int, int> counts;

                if (selectedType == "Categories")
                {
                    counts = _repository.GetMovieCountsByCategories(ids);
                }
                else
                {
                    var positionName = GetPositionName(selectedType)!;
                    var positionId = _repository.GetPositionId(positionName);
                    counts = _repository.GetMovieCountsByPeople(ids, positionId);
                }

                foreach (var id in ids)
                {
                    var item = vm.AvailableItems.FirstOrDefault(i => i.Value == id.ToString());
                    vm.Labels.Add(item?.Text ?? "Unknown");
                    vm.Counts.Add(counts.GetValueOrDefault(id, 0));
                }
            }

            return vm;
        }
    }
}
