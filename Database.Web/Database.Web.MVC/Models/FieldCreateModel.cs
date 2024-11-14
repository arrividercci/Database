using Database.Common.Models;

namespace Database.Web.MVC.Models
{
    public class FieldCreateModel
    {
        public int Index { get; set; }
        public string TableName { get; set; }
        public string DbName { get; set; }
        public List<Property> Columns { get; set; } = new();
        public Dictionary<string, dynamic> Values { get; set; } = new();
    }
}
