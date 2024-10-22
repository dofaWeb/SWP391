﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;


namespace SWP391_FinalProject.Controllers
{
    public class OrderManController : Controller
    {
        public IActionResult Display()
        {
            return View();
        }

        public IActionResult ListOrder()
        {
            Repository.OrderRepository orderRepo = new Repository.OrderRepository();
            var orderDetail = orderRepo.GetAllOrder();
            return View(orderDetail);
        }

        public IActionResult History()
        {
            Repository.OrderRepository orderRepo = new OrderRepository();
            var OrderHis = orderRepo.GetAllOrder();
            return View(OrderHis);
        }
       

    }
}
