using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieMVC.Models.ViewModels
{
    public class MovieFormViewModel
    {
        public Movie Movie { get; set; } = new();
        public MultiSelectList? Directors { get; set; }
        public MultiSelectList? Producers { get; set; }
        public MultiSelectList? Writers { get; set; }
        public MultiSelectList? Actors { get; set; }
        public MultiSelectList? Categories { get; set; }
    }
}
