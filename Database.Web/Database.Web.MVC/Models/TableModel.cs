using Database.Common.Models;

namespace Database.Web.MVC.Models
{
    public class TableModel
    {
        public string DatabaseName { get; set; }
        public string Name { get; set; }
        public List<Property> Properties { get; set; } = new();
    }
}
