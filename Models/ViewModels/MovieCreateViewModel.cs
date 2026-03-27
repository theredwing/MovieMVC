using Microsoft.AspNetCore.Mvc.Rendering;
using MovieMVC.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models.ViewModels
{
    public class MovieCreateViewModel
    {
        [Required] public MovieMVC.Models.Movie Movie { get; set; }
        public MovieDto? MovieDto { get; set; }
        public IEnumerable<SelectListItem>? Directors { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<SelectListItem>? Actors { get; set; }
        public IEnumerable<SelectListItem>? Writers { get; set; }
        public IEnumerable<SelectListItem>? Producers { get; set; }
        [Required] public string Title { get; set; }

        public int? DirectorId { get; set; }
        public int? CategoryId { get; set; }
    }
}
