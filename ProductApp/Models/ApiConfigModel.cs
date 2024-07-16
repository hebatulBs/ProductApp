namespace ProductApp.Models
{
    public class ApiConfigModel
    {
        public string BaseUrl { get; set; }
        public string AuthenticationEndpoint { get; set; }
        public string GetAllProductsEndpoint { get; set; }
        public string CreateOrEditProductEndpoint { get; set; }
        public string TenantId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
