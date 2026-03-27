namespace MovieMVC.DTOs
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string> Actors { get; set; } = [];
        public List<string> Directors { get; set; } = [];
        public List<string> Writers { get; set; } = [];
        public List<string> Producers { get; set; } = [];
        public List<string> Categories { get; set; } = [];
    }
}
