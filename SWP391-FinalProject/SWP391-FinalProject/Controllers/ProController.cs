using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Controllers
{
    public class ProController : Controller
    {
        private readonly Swp391Se1801Group3Context db;

        public ProController(Swp391Se1801Group3Context context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var Products  = db.Products.AsQueryable();
            var result = Products.Select(p => new Models.Product
            {
                Name = p.Name,
                Picture = p.Picture,
                Brand = p.BrandId ,
                Description = p.Description
            });
            return View(result);
        }

        public IActionResult ProductDetail(string id)
        {
            return View();
        }
    }
}
