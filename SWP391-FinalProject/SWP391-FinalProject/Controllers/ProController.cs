
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Filters;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Data;
using System.Security.Claims;

namespace SWP391_FinalProject.Controllers
{
    public class ProController : Controller
    {

        public ProController()
        {


        }

        public async Task<IActionResult> Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Role0001" || userRole == "Role0002")
            {
                return RedirectToAction("Index", "ProMan");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
            return View();
        }

        public async Task<IActionResult> Profile(string username)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }

            Repository.UserRepository userRepo = new Repository.UserRepository();
            UserModel user = new UserModel();
            user = userRepo.GetUserProfileByUsername(username);
            
            return View(user);

        }

        [HttpGet]
        public async Task<IActionResult> ProductDetail(string id, string productItemId, decimal Price)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
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
                string check = "SELECT * FROM Product_Item Where id = @proItemId And quantity > 0";
                var parameter = new Dictionary<string, object>
                {
                    { "@proItemId", item }
                };
                var result = DataAccess.DataAccess.ExecuteQuery(check,parameter);
                if (result.Rows.Count > 0)
                {
                    string ramOption = prodp.GetProductVariationOption(item, "Ram");
                    string storageOption = prodp.GetProductVariationOption(item, "Storage");
                    // Combine RAM and Storage into a single option string
                    string combinedOption = $"RAM: {ramOption} <br/> Storage: {storageOption} ";
                    // Add to dictionary with the combined option as both the key and value (just for consistency)
                    option[item] = combinedOption;
                }
            }
            ViewBag.Option = option;
            // Combine the product and comments into a ViewModel
            Repository.ComRepository commentRep = new Repository.ComRepository();
            ViewBag.ProductItemId = productItemId;
            ViewBag.Price = Price;  
            var comments = commentRep.GetCommentsByProductId(id); // Get comments
            string branchId = prodp.GetBrandId(id);
            var RelatedProduct = prodp.GetProductByBrand4(branchId,id);
            ViewBag.RelatedProduct = RelatedProduct;
            ViewBag.Comments = comments;

            Repository.RatingRepository ratingRepo = new Repository.RatingRepository();
            double averageRating = ratingRepo.GetAverageRating(id);  // Get the average rating for the product
            
            ViewBag.AverageRating = averageRating;

            
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
        public IActionResult SearchedProduct(string keyword, string sortByPrice, string sortByCat, string sortByBrand)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
            // Use the repository to get products matching the keyword
            //Repository.Product proRepo = new Repository.Product(db);
            //var products = proRepo.GetProductsByKeyword(keyword);

            ViewBag.keyword = keyword;
            ViewBag.sortByPrice = sortByPrice;
            ViewBag.sortByCat = sortByCat;
            ViewBag.sortByBrand = sortByBrand;
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
        public void Rating([FromBody] RatingModel rating)
        {
            if (rating != null)
            {
                UserRepository userRepo = new UserRepository();

                // Suy từ username ra UserId
                string userId = userRepo.GetUserIdByUserName(rating.Username); // Đổi từ GetUserIdByUsername thành GetUserIdByUserName
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("Invalid username.");
                }

                rating.UserId = userId; // Gán UserId lấy được vào rating
                RatingRepository ratingRepo = new RatingRepository();
                ratingRepo.InsertOrUpdateRating(rating);
            }
        }


    }

}

