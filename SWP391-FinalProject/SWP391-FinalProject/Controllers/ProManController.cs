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

        public IActionResult Index()
        {
            StatisticsRepository statisticsRepository = new StatisticsRepository();
            var stats = statisticsRepository.GetImportPrice();
            // Filter based on the year
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            switch (role)
            {
                case "Role0001":
                case "Role0002":
                    return View();
                  
                case "Role0003":
                default:
                    return RedirectToAction("Index", "Pro");
                
            }
        }

        // Method to filter the data by the selected year
        private List<StatisticsModel> GetImportPriceByYear(int year)
        {
            StatisticsRepository statisticsRepository = new StatisticsRepository();
            // Filter based on the year
            var filteredData = statisticsRepository.GetImportPrice().Where(stat => stat.ChangeDate.Year == year).ToList();
            return filteredData;
        }

        public IActionResult Display()
        {
            ProductRepository proRepo = new ProductRepository();
            var query = proRepo.GetAllProduct();

            return View(query);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            ProductRepository proRepo = new ProductRepository();
            CategoryRepository catRepo = new CategoryRepository();

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
            ProductRepository proRepo = new ProductRepository();
            proRepo.AddProduct(model, pictureUpload);
            TempData["SuccessMessage"] = "Category added successfully.";
            return RedirectToAction("Display");
        }

        [HttpGet]
        public IActionResult EditProduct(string id)
        {
            ProductRepository proRepo = new ProductRepository();
            CategoryRepository catRepo = new CategoryRepository();
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
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProduct(model, pictureUpload);

            return RedirectToAction("Display");
        }

        [HttpPost]
        public IActionResult AddProItem(ProductItemModel model)
        {

            ProductItemRepository proRepo = new ProductItemRepository();
            TempData["SuccessMessage"] = "Product item added successfully.";
            proRepo.AddProductItem(model);
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
            proRepo.Delete(id);
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
    }
}