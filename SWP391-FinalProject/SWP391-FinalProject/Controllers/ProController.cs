﻿using Microsoft.AspNetCore.Mvc;
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
            Repository.Product proRepo = new Repository.Product(db);
            var result = proRepo.GetAllProduct();
            return View(result);
        }

        public IActionResult ProductDetail(string id)
        {
            return View();
        }
        [HttpGet]
        public IActionResult SearchedProduct(string keyword)
        {
            // Use the repository to get products matching the keyword
            Repository.Product proRepo = new Repository.Product(db);
            var products = proRepo.GetProductsByKeyword(keyword);

            // Return the view with the list of products
            return View(products); // Adjust this based on your view's structure
        }
    }
}
