namespace CategoryProcessor.Models
{
    public class Category
    {
        public string CategoryName { get; set; }
        public List<SubCategory> SubCategories { get; set; }
    }
}
