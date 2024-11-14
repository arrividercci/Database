using Database.Common.Models;

namespace Database.Web.MVC.Models
{
    public class TableFullModel
    {
        public string TableName { get; set; }
        public string DbName { get; set; }
        public List<Property> Columns { get; set; } = new();
        public List<List<string>> Values { get; set; } = new();
    }
}
