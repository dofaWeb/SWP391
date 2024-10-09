using Microsoft.AspNetCore.Mvc;
namespace SWP391_FinalProject.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
