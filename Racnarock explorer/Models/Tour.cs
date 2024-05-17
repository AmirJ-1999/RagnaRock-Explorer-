namespace Racnarock_explorer.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Default to empty string to avoid null
        public string Description { get; set; } = string.Empty; // Default to empty string to avoid null
        public string AudioUrl { get; set; } = string.Empty; // Default to empty string to avoid null
    }
}
