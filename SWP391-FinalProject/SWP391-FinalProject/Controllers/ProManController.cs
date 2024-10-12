using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Filters;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SWP391_FinalProject.Controllers
{
    [ServiceFilter(typeof(ProManAuthorizationFilter))]
    public class ProManController : Controller
    {
        private readonly DBContext db;

        public ProManController(DBContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if(role != "Role0001")
            {
                return RedirectToAction("Index", "Pro");
            }
            return View();
        }

        public IActionResult Display()
        {
            Repository.ProductRepository proRepo = new Repository.ProductRepository(db);
            var query = proRepo.GetAllProduct();

            return View(query);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            Repository.ProductRepository proRepo = new Repository.ProductRepository(db);
            Repository.CategoryRepository catRepo = new Repository.CategoryRepository(db);

            ViewBag.NewProductId = proRepo.getNewProductID();
            //ViewBag.Category = catRepo.GetAllCategory();
            ViewBag.ProductStates = proRepo.getAllProductState();
            ViewBag.Laptops = catRepo.GetAllCatLaps();
            ViewBag.Smartphones = catRepo.GetAllCatPhone();
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Models.ProductModel model, IFormFile pictureUpload)
        {
            Repository.ProductRepository proRepo = new Repository.ProductRepository(db);
            proRepo.AddProduct(model, pictureUpload);
            return RedirectToAction("Display");
        }
    }
}