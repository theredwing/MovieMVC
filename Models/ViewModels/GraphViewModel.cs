using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieMVC.Models.ViewModels
{
    public class GraphViewModel
    {
        public string? SelectedType { get; set; }
        public List<SelectListItem> AvailableItems { get; set; } = [];
        public int[]? SelectedIds { get; set; }
        public List<string> Labels { get; set; } = [];
        public List<int> Counts { get; set; } = [];
        public string? Sort { get; set; }
        public bool? Desc { get; set; }
        public string? Search { get; set; }
    }
}
