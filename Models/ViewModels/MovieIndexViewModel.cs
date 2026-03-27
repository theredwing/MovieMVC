namespace MovieMVC.Models.ViewModels
{
    public class MovieIndexViewModel
    {
        public List<MovieCreateViewModel> Movies { get; set; } = [];
        public string? CurrentSort { get; set; }
        public bool NextSort { get; set; }
        public string? Search { get; set; }
    }
}
