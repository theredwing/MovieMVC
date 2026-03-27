using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models
{
    public class NamesLU
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public List<MovieNamesPosition>? MoviePeople { get; set; } = new();
    }
}
