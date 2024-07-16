using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using ProductApp.Filters;
using ProductApp.Models;
using ProductApp.Services;
using ProductApp.ViewModels;

namespace ProductApp.Controllers
{
    public class ProductController : Controller
    {
        #region fields
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiConfigModel _apiSettings;
        #endregion

        #region ctor
        public ProductController(ILogger<ProductController> logger, IProductService productService, IHttpContextAccessor httpContextAccessor, IOptions<ApiConfigModel> apiSettings)
        {
            _logger = logger;
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;

        }
        #endregion

        #region authentication
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!string.IsNullOrEmpty(GetToken()))
            {
                ViewBag.IsAuthenticated = true;
            }
            return View(new AuthViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(AuthViewModel model)
        {
            if (model.ButtonPressed)
            {
                string token = await _productService.AuthenticateAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpContextAccessor.HttpContext.Session.SetString("AccessToken", token);
                    TempData["Notification"] = "Authenticated!";
                    TempData["NotificationType"] = "success";
                    return RedirectToAction("Lists", "Product");
                }

                TempData["Notification"] = "Not authenticated!";
                TempData["NotificationType"] = "error";

            }
            return View("Index");
        }
        public IActionResult LogOut()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        #endregion

        #region get/create/update

        [TypeFilter(typeof(AccessTokenFilter))]
        public async Task<IActionResult> Lists(int pageNumber = 1, int pageSize = 10)
        {
            var products = await _productService.GetAllProductsAsync(pageNumber, pageSize, GetToken());
            ViewBag.PageSize = pageSize;
            return View(products);
        }

        [TypeFilter(typeof(AccessTokenFilter))]
        public IActionResult Create()
        {
            if (!string.IsNullOrEmpty(GetToken()))
            {
                return View(new ProductViewModel());
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [TypeFilter(typeof(AccessTokenFilter))]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product()
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsAvailable = model.IsAvailable,
                    TenantId = Convert.ToInt32(_apiSettings.TenantId)
                };
                var response = await _productService.CreateOrUpdateProductAsync(product, GetToken());
                if (response.Success)
                {
                    TempData["Notification"] = response.Data;
                    TempData["NotificationType"] = "success";
                    return RedirectToAction(nameof(Lists));
                }
                else
                {
                    TempData["Notification"] = response.Message;
                    TempData["NotificationType"] = "error";
                    return View(model);
                }
            }
            return View(model);
        }

        [TypeFilter(typeof(AccessTokenFilter))]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id, GetToken());
            if (product == null)
            {
                TempData["Notification"] = "Product not found.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Lists");
            }
            var model = new ProductViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                IsAvailable = product.IsAvailable,
                TenantId = product.TenantId
            };
            return View(model);
        }

        [HttpPost]
        [TypeFilter(typeof(AccessTokenFilter))]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product()
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsAvailable = model.IsAvailable,
                    TenantId = Convert.ToInt32(_apiSettings.TenantId),
                    Id = model.Id
                };
                var response = await _productService.CreateOrUpdateProductAsync(product, GetToken());
                if (response.Success)
                {
                    TempData["Notification"] = response.Data;
                    TempData["NotificationType"] = "success";
                    return RedirectToAction(nameof(Lists));
                }
                else
                {
                    TempData["Notification"] = response.Message;
                    TempData["NotificationType"] = "error";
                    return View(model);
                }
            }
            return View(model);
        }
        #endregion

        #region token
        private string GetToken()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
            return string.IsNullOrEmpty(token) ? string.Empty : token;
        }
        #endregion

    }
}
