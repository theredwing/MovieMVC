using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models
{
    public class MovieCategory
    {
        public int Id { get; set; }
        [Required] public int MovieId { get; set; }
        [Required] public int CategoryId { get; set; }
        public CategoryLU Category { get; set; }
        public Movie Movie { get; set; }
    }
}
