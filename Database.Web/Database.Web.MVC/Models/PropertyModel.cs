using Database.Common.Models;

namespace Database.Web.MVC.Models
{
    public class PropertyModel
    {
        public string DbName { get; set; }
        public string TableName { get; set; }
        public PropertyType PropertyType { get; set; }
        public string Name { get; set; }
    }
}
