
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
            db = new DBContext();
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
            // Initialize repositories
            Repository.ProductRepository prodp = new Repository.ProductRepository();
          

            // Fetch the product by ID
            ProductModel p = prodp.GetProductById(id);

            var proItemId = prodp.GetProductItemIdByProductId(id);

            HashSet<string> RamList = new HashSet<string>();
            HashSet<string> StorageList = new HashSet<string>();

            foreach(var item in proItemId)
            {
                RamList.Add(prodp.GetProductVariationOption(item, "Ram"));
                StorageList.Add(prodp.GetProductVariationOption(item, "Storage"));
            }

            ViewBag.RamList = RamList;
            ViewBag.StorageList = StorageList;

            // Combine the product and comments into a ViewModel

            Repository.ComRepository commentRep = new Repository.ComRepository();
            var comments = commentRep.GetCommentsByProductId(id); // Get comments
            ViewBag.Comments = comments;
            // Return the view with the combined model
            return View(p);
        }
        [HttpGet]
        public JsonResult GetPrice(string ram, string storage, string productId)
        {
            ProductRepository proRepo = new ProductRepository();
            var model = proRepo.GetPrice(ram, storage, productId);

            // Ensure the response is in the right structure
            if (model != null)
            {
                return Json( model );
            }
            else
            {
                return Json( "Not available");
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
            ViewBag.brand=brand;
            return View();
        }
    }
}
