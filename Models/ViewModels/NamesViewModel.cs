namespace MovieMVC.Models.ViewModels
{
    public class NamesViewModel
    {
        public List<NameEntry> Names { get; set; } = [];
        public string? Sort { get; set; }
        public string? Desc { get; set; }
        public string? Search { get; set; }
        public string? FocusNameId { get; set; }
    }

    public class NameEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool CanDelete { get; set; }
    }
}
