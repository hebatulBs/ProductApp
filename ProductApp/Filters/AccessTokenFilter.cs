using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductApp.Filters
{
    public class AccessTokenFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Index", "Product", null);
            }
        }
    }
}
