using System.ComponentModel.DataAnnotations;

namespace MovieMVC.Models
{
    public class MovieNamesPosition
    {
        // composite key (MovieId, NamesLUID) configured in AppDbContext
        public int Id { get; set; }
        [Required] public int MovieId { get; set; }
        [Required] public int NamesId { get; set; }
        [Required] public int PositionId { get; set; }

        public Movie Movie { get; set; }
        public NamesLU Name { get; set; }
        public PositionLU Position { get; set; }
    }
}
