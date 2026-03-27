using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models
{
    public class CategoryLU
    {
        public int Id { get; set; }
        [Required] public string Category { get; set; }
        public List<MovieCategory> MovieCategories { get; set; } = new();
    }
}
