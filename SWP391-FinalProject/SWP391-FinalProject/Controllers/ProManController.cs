﻿using Microsoft.AspNetCore.Mvc;
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
        public ProManController()
        {

        }

        public IActionResult Index(int year = 2024)
        {
            StatisticsRepository statisticsRepository = new StatisticsRepository();
            var sellingStat = statisticsRepository.GetSellingPrice(year);
            var importStat = statisticsRepository.GetImportPrice(year);

            var profitStat = ((IEnumerable<dynamic>)sellingStat).Select((s, index) => new
            {
                Month = s.Month,
                Profit = s.TotalSellingPrice - ((IEnumerable<dynamic>)importStat).ElementAtOrDefault(index)?.TotalImportPrice ?? 0
            }).ToList();

            // Filter based on the year
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            switch (role)
            {
                case "Role0001":
                    ViewBag.SellingPrices = sellingStat;
                    ViewBag.ImportPrices = importStat;
                    ViewBag.ProfitStats = profitStat;
                    return View();
                case "Role0002":
                    var Username = User.FindFirst(ClaimTypes.Name)?.Value;
                    return RedirectToAction("Schedule", "StaffMan", new { Username = Username});
                  
                case "Role0003":
                default:
                    return RedirectToAction("Index", "Pro");
                
            }
        }
        [HttpGet]
        public IActionResult Display()
        {
            ProductRepository proRepo = new ProductRepository();
            var query = proRepo.GetAllProductAdmin();
            ViewBag.AvailableCount = query.Count(o => o.ProductState == "Available");
            ViewBag.UnAvailableCount = query.Count(o => o.ProductState == "Unavailable");
            ViewBag.OutOfStockCount = query.Count(o => o.ProductState == "Out of Stock");
            return View(query);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            ProductRepository proRepo = new ProductRepository();
            CategoryRepository catRepo = new CategoryRepository();

            ViewBag.NewProductId = proRepo.getNewProductID();
            ViewBag.Category = catRepo.GetAllCategory();
            ViewBag.ProductStates = proRepo.GetAllProductState();
            ViewBag.Laptops = catRepo.GetAllCatLaps();
            ViewBag.Smartphones = catRepo.GetAllCatPhone();
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Models.ProductModel model, IFormFile pictureUpload)
        {
            ProductRepository proRepo = new ProductRepository();
            if (proRepo.AddProduct(model, pictureUpload))
            {
                TempData["SuccessMessage"] = "Product added successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Product added fail due to duplicated";
            }
            return RedirectToAction("Display");
        }

        [HttpGet]
        public IActionResult EditProduct(string id)
        {
            ProductRepository proRepo = new ProductRepository();
            CategoryRepository catRepo = new CategoryRepository();
            var proInfor = proRepo.GetProductById(id);

            var proState = proRepo.GetAllProductState();

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
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProduct(model, pictureUpload);

            return RedirectToAction("Display");
        }

        [HttpPost]
        public IActionResult AddProItem(ProductItemModel model)
        {
            ProductItemRepository proRepo = new ProductItemRepository();
            if (proRepo.AddProductItem(model))
            {
                TempData["SuccessMessage"] = "Option added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Option added fail due to duplicated!";

            }
            return RedirectToAction("EditProduct", new { id = model.ProductId });
        }

        [HttpPost]
        public IActionResult EditProductItem(ProductItemModel model)
        {
            ProductItemRepository proRepo = new ProductItemRepository();
            proRepo.EditProductItem(model);
            return RedirectToAction("EditProduct", new { id = model.ProductId });
        }

        public IActionResult Disable(string id)
        {
            ProductRepository proRepo = new ProductRepository();
            proRepo.Disable(id);
            return RedirectToAction("Display");
        }

        public IActionResult DeleteProductItem(string id, string productId)
        {
            ProductItemRepository proRepo = new ProductItemRepository();
            TempData["Message"] = proRepo.Delete(id);
            return RedirectToAction("EditProduct", new { id = productId });
        }

        public IActionResult ImportProductItem(string VariationImport, int Quantity, string productId)
        {
            ProductItemRepository proRepo = new ProductItemRepository();
            proRepo.Import(VariationImport, Quantity);
            return RedirectToAction("EditProduct", new { id = productId });
        }

        [HttpGet]
        public IActionResult ProductLog()
        {
            ProductRepository proRepo = new ProductRepository();
            var result = proRepo.GetProductLog();
            return View(result);
        }

        [HttpPost]
        public IActionResult ProductLog(string Filter)
        {
            ProductRepository proRepo = new ProductRepository();
            var result = proRepo.GetProductLog(Filter);
            return View(result);
        }
        [HttpPost]
        public IActionResult Display(string keyword)
        {
            ProductRepository proRepo = new ProductRepository();
            var querry=proRepo.GetAllProductByKeyword(keyword);
            return View(querry);
        }


        public IActionResult DeleteProduct(string id)
        {
            ProductRepository proRepo = new ProductRepository();
            if(proRepo.DeleteProduct(id))
            {
                TempData["SuccessMessage"] = "Deleted Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot delete due to this product has been purchased by customer";
            }
            return RedirectToAction("Display");
        }
    }
}