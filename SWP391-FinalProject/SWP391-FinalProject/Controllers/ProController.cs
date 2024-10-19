
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Filters;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Security.Claims;

namespace SWP391_FinalProject.Controllers
{
    public class ProController : Controller
    {
        private readonly DBContext db;

        public ProController()
        {


        }

        public bool CheckLoginCookie()
        {
            string cookie = HttpContext.Request.Cookies["Username"];
            return cookie != null && cookie.Length > 0;
        }


        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("username") == null)
            {
                if (CheckLoginCookie())
                {
                    HttpContext.Session.SetString("username", HttpContext.Request.Cookies["Username"]);
                    return RedirectToAction("LoginWithCookie", "Acc", new { username = HttpContext.Request.Cookies["Username"] });
                }
            }
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

        public async Task<IActionResult> ProductDetail(string id)
        {
            Repository.ProductRepository prodp = new Repository.ProductRepository();
            ProductModel p = prodp.GetProductById(id);
            ProductItemModel productModel = prodp.GetProductItemById(id);

            return View(productModel);

        }
        [HttpGet]
        public JsonResult GetPrice(string combinedOption, string productId)
        {
            ProductRepository proRepo = new ProductRepository();
            var parts = combinedOption.Split(new string[] { "RAM: ", "<br/> Storage: " }, StringSplitOptions.None);


            string ram = parts[1];
            string storage = parts[2];
            var price = proRepo.GetPrice(ram, storage, productId);

            // Ensure the response is in the right structure
            if (price != null)
            {
                return Json(price);
            }
            else
            {
                return Json("Not available");
            }
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
            ViewBag.brand = brand;
            return View();
        }
        [HttpPost]
        public void Rating([FromBody] RatingModel Rating)
        {
            if (Rating != null)
            {
                ProductRepository productRepo = new ProductRepository();
                Rating.ProductId = productRepo.GetProductIdByProductItemId(Rating.ProductItemId);
                Rating.Rating += 1;
                RatingRepository ratingRepo = new RatingRepository();
                ratingRepo.InsertRating(Rating);
            }
        }
        


    }

}

