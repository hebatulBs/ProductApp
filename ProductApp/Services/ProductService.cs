using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductApp.Models;
using System.Drawing.Printing;
using System.Net.Http.Headers;
using System.Text;

namespace ProductApp.Services
{
    public class ProductService : IProductService
    {
        #region fields
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly ApiConfigModel _apiSettings;
        private string _accessToken = string.Empty;
        #endregion

        #region ctor
        public ProductService(HttpClient httpClient, ILogger<ProductService> logger, IOptions<ApiConfigModel> apiSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiSettings = apiSettings.Value;
        }
        #endregion

        #region authentication
        public async Task<string> AuthenticateAsync()
        {

            string accessToken = string.Empty;
            var requestBody = new
            {
                userNameOrEmailAddress = _apiSettings.UserName,
                password = _apiSettings.Password
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Add("Abp.TenantId", _apiSettings.TenantId);
            var response = await _httpClient.PostAsync(_apiSettings.BaseUrl + _apiSettings.AuthenticationEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var authResponse = JsonConvert.DeserializeObject<AccessToken>(responseString);
                accessToken = authResponse is null ? string.Empty : authResponse.result.accessToken;
            }
            else
            {
                _logger.LogError("Authentication failed: " + response.ReasonPhrase);
            }

            return accessToken;
        }
        #endregion

        #region methods
        public async Task<PagedList<Product>> GetAllProductsAsync(int pageNumber, int pageSize, string token)
        {
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(_apiSettings.BaseUrl + _apiSettings.GetAllProductsEndpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var products = JsonConvert.DeserializeObject<List<Product>>(responseString);
                    if (products == null)
                    {
                        _logger.LogError("Deserialization returned null.");
                        throw new Exception("Error deserializing product data.");
                    }
                    return PagedList<Product>.Create(products, pageNumber, pageSize);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError("JSON deserialization error: " + jsonEx.Message);
                    return PagedList<Product>.Create(new List<Product>(), pageNumber, pageSize, error: jsonEx.Message);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching products: " + errorContent);
                return PagedList<Product>.Create(new List<Product>(), pageNumber, pageSize, error: "An error occured.");
            }
        }

        public async Task<Product> GetProductByIdAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(_apiSettings.BaseUrl + _apiSettings.GetAllProductsEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var products = JsonConvert.DeserializeObject<List<Product>>(responseString);
                    if (products == null)
                    {
                        _logger.LogError("Deserialization returned null.");
                        return new Product();
                    }
                    var product = products.FirstOrDefault(p => p.Id == id);
                    return product is null? new Product() : product;
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError("JSON deserialization error: " + jsonEx.Message);
                }
            }
            return new Product();
        }
      
        public async Task<ApiResponse<string>> CreateOrUpdateProductAsync(Product product , string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiSettings.BaseUrl + _apiSettings.CreateOrEditProductEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<string> 
                { Success = true, Data = $"Product {product.Name} created/updated successfully." };
            }
            else
            {
                _logger.LogError("Error creating/updating product: " + response.ReasonPhrase);
                return new ApiResponse<string> { Success = false, Message = "Error creating/updating product" };
            }
        }
        #endregion
    }
}
