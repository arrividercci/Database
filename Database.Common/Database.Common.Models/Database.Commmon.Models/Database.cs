namespace Database.Common.Models
{
    public class Database
    {
        public required string Name { get; set; }
        public List<Table> Tables { get; set; } = new();
    }
}
