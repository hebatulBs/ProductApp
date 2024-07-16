namespace ProductApp.Models
{
    public class Product
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public int Id { get; set; }
    }
}
