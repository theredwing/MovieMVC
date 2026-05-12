namespace MovieMVC.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; } = new();
        public string? Sort { get; set; }
        public string? Desc { get; set; }
        public string? Search { get; set; }
    }
}
