using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Controllers
{
    public class ProController : Controller
    {
        private readonly DBContext db;

        public ProController(DBContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var Products  = db.Products.AsQueryable();
            var result = Products.Select(p => new Models.ProductModel
            {
                Name = p.Name,
                Picture = p.Picture,
                CategoryId = p.CategoryId ,
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
