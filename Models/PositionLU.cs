using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models
{
    public class PositionLU
    {
        public int Id { get; set; }
        [Required] public string Position { get; set; }
        public List<MovieNamesPosition>? MoviePeople { get; set; } = new();
    }
}
