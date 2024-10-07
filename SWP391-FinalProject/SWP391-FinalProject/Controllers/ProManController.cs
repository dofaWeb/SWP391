﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Net.Http.Headers;

namespace SWP391_FinalProject.Controllers
{
    public class ProManController : Controller
    {
        private readonly Swp391Se1801Group3Context db;

        public ProManController(Swp391Se1801Group3Context context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Display()
        {
            var query = from p in db.Products
                        join pi in db.ProductItems on p.Id equals pi.ProductId
                        join b in db.Brands on p.BrandId equals b.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        group pi by new { p.Id, p.Name, p.Picture, p.Description, Brand = b.Name, State = ps.Name } into g
                        select new Models.Product
                        {
                            Id = g.Key.Id,
                            Name = g.Key.Name,
                            Picture = g.Key.Picture,
                            Description = g.Key.Description,
                            Brand = g.Key.Brand,
                            State = g.Key.State,
                            Quantity = g.Sum(pi => pi.Quantity)
                        };

            return View(query);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(object model) => RedirectToAction("Display");
    }
}