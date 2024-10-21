
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
            if (HttpContext.Session.GetString("Username") == null)
            {
                if (CheckLoginCookie())
                {
                    HttpContext.Session.SetString("Username", HttpContext.Request.Cookies["Username"]);
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
        public async Task<IActionResult> ProductDetail(string id, string productItemId, decimal Price)
        {
            // Initialize repositories
            Repository.ProductRepository prodp = new Repository.ProductRepository();
            // Fetch the product by ID
            ProductModel p = prodp.GetProductById(id);
            p.ProductItem = new ProductItemModel { Id = productItemId };
            p.MinPrice = Price;
            var proItemId = prodp.GetProductItemIdByProductId(id);
            Dictionary<string, string> option = new Dictionary<string, string>();
            foreach (var item in proItemId)
            {
                string ramOption = prodp.GetProductVariationOption(item, "Ram");
                string storageOption = prodp.GetProductVariationOption(item, "Storage");
                // Combine RAM and Storage into a single option string
                string combinedOption = $"RAM: {ramOption} <br/> Storage: {storageOption} ";
                // Add to dictionary with the combined option as both the key and value (just for consistency)
                option[item] = combinedOption;
            }
            ViewBag.Option = option;
            // Combine the product and comments into a ViewModel
            Repository.ComRepository commentRep = new Repository.ComRepository();
            var comments = commentRep.GetCommentsByProductId(id); // Get comments
            string branchId = prodp.GetBrandId(id);
            var RelatedProduct = prodp.GetProductByBrand4(branchId,id);
            ViewBag.RelatedProduct = RelatedProduct;
            ViewBag.Comments = comments;
            // Return the view with the combined model
            return View(p);
            
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
                RatingRepository ratingRepo = new RatingRepository();
                ratingRepo.InsertRating(Rating);
            }
        }
        
    }

}

