using ProductApp.Models;

namespace ProductApp.Services
{
    public interface IProductService
    {
        Task<string> AuthenticateAsync();
        Task<PagedList<Product>> GetAllProductsAsync(int pageNumber, int pageSize, string token);
        Task<ApiResponse<string>> CreateOrUpdateProductAsync(Product product, string token);
        Task<Product> GetProductByIdAsync(int id, string token);
    }
}
