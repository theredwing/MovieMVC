namespace MovieMVC.DTOs
{
    public class MovieSelections
    {
        public int[] DirectorIds { get; set; } = [];
        public int[] ProducerIds { get; set; } = [];
        public int[] WriterIds { get; set; } = [];
        public int[] ActorIds { get; set; } = [];
        public int[] CategoryIds { get; set; } = [];
    }
}
