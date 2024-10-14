using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Filters;
using SWP391_FinalProject.Models;
using System.Security.Claims;

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
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Role0001" || userRole == "Role0002")
            {
                return RedirectToAction("Index", "ProMan");
            }
            return View();
        }
        public async Task<IActionResult> Profile(string username)
        {
            Repository.UserRepository userRepo = new Repository.UserRepository();
            UserModel user = new UserModel();
            user = userRepo.GetUserProfileByUsername(username);

            return View(user);

        }

        [HttpGet]
        public async Task <IActionResult> ProductDetail(string id)
        {
           Repository.ProductRepository prodp= new Repository.ProductRepository();
            ProductModel p = prodp.GetProductById(id);
            ProductItemModel productModel= prodp.GetProductItemById(id);
           
            return View(productModel);
        }
        [HttpGet]
        public IActionResult SearchedProduct(string keyword)
        {
            // Use the repository to get products matching the keyword
            //Repository.Product proRepo = new Repository.Product(db);
            //var products = proRepo.GetProductsByKeyword(keyword);

            ViewBag.keyword = keyword;
            // Return the view with the list of products
            return View(); // Adjust this based on your view's structure
        }
        public IActionResult ProductsByCategory(string type)
        {
            ViewBag.type = type;
            return View();
        }
        public IActionResult GetProductByBrand(string brand)
        {
            ViewBag.brand=brand;
            return View();
        }
    }
}
