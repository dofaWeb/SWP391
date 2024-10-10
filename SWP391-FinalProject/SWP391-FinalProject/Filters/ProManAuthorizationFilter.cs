using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SWP391_FinalProject.Filters
{
    public class ProManAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            
            // Get the controller name
            var controllerName = context.RouteData.Values["controller"].ToString();

            // If the controller name starts with "ProMan"
            if (controllerName.StartsWith("ProMan"))
            {
                // Get the user's role from claims
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

                // Check if the role is not Role0001 or Role0002
                if (userRole != "Role0001" && userRole != "Role0002")
                {
                    // Redirect to Index action of the Pro controller
                    context.Result = new RedirectToActionResult("Index", "Pro", null);
                    return;
                }
            }
        }
    }
}
