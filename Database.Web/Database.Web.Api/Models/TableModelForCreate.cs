using Database.Common.Models;

namespace Database.Web.Api.Models
{
    public class TableModelForCreate
    {
        public string Name { get; set; }
        public List<Property> Properties { get; set; }
    }
}
