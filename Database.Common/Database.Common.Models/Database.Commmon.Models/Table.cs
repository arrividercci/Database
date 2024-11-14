namespace Database.Common.Models
{
    public class Table
    {
        public required string Name { get; set; }
        public List<Property> Properties { get; set; } = new();
        public List<List<string>> Data { get; set; } = new List<List<string>>();
    }
}
