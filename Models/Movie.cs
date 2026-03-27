using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MovieMVC.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = "";   
        public string? Description { get; set; }

        [ValidateNever]
        public List<MovieNamesPosition>? MoviePeople { get; set; } = new ();
        [ValidateNever]
        public List<MovieCategory>? MovieCategory { get; set; } = new ();
    }
}
    