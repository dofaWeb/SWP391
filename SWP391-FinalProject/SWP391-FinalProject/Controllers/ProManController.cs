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
            if (role != "Role0001")
            {
                return RedirectToAction("Index", "Pro");
            }
            return View();
        }

        public IActionResult Display()
        {
            ProductRepository proRepo = new ProductRepository(db);
            var query = proRepo.GetAllProduct();

            return View(query);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            ProductRepository proRepo = new ProductRepository(db);
            CategoryRepository catRepo = new CategoryRepository(db);

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
            ProductRepository proRepo = new ProductRepository(db);
            proRepo.AddProduct(model, pictureUpload);
            return RedirectToAction("Display");
        }

        [HttpGet]
        public IActionResult EditProduct(string id)
        {
            ProductRepository proRepo = new ProductRepository(db);
            CategoryRepository catRepo = new CategoryRepository(db);
            var proInfor = proRepo.GetProductById(id);

            var proState = proRepo.getAllProductState();

            ViewBag.Product = proInfor;
            ViewBag.Laptops = catRepo.GetAllCatLaps();
            ViewBag.Smartphones = catRepo.GetAllCatPhone();
            ViewBag.ProductState = proState;
            ViewBag.ProductItem = proRepo.GetProductItem(id);
            return View();
        }
        [HttpPost]
        public IActionResult EditProduct(ProductModel model, IFormFile pictureUpload)
        {
            ProductRepository proRepo = new ProductRepository(db);
            proRepo.UpdateProduct(model, pictureUpload);

            return RedirectToAction("Display");
        }

        [HttpPost]
        public IActionResult AddProItem(ProductItemModel model)
        {
            ProductItemRepository proRepo = new ProductItemRepository(db);
            proRepo.AddProductItem(model);
            return RedirectToAction("EditProduct", new { id = model.ProductId });
        }

        [HttpPost]
        public IActionResult EditProductItem(ProductItemModel model)
        {
            ProductItemRepository proRepo = new ProductItemRepository(db);
            proRepo.EditProductItem(model);
            return RedirectToAction("EditProduct", new { id = model.ProductId });
        }

        public IActionResult Disable(string id)
        {
            ProductRepository proRepo = new ProductRepository(db);
            proRepo.Disable(id);
            return RedirectToAction("Display");
        }

        public IActionResult DeleteProductItem(string id, string productId)
        {
            ProductItemRepository proRepo = new ProductItemRepository(db);
            proRepo.Delete(id);
            return RedirectToAction("EditProduct", new { id = productId });
        }
    }
}