namespace Database.Web.MVC.Models
{
    public class JoinTablesModel
    {
        public string DatabaseName { get; set; }
        public string FirstTableName { get; set; }
        public string SecondTableName { get; set; }
        public string FirstTableParameter { get; set; }

        public string SecondTableParameter { get; set; }
    }
}
