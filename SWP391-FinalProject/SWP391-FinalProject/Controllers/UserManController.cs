using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mysqlx.Crud;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Runtime.Intrinsics.Arm;

namespace SWP391_FinalProject.Controllers
{
    public class UserManController : Controller
    {

        public UserManController()
        {

        }
        [Authorize]
        public IActionResult Display()

        {
            AccountRepository AccRepo = new AccountRepository();
            var user = AccRepo.GetAllAccount();
            return View(user);
        }
        [Authorize]

        public IActionResult ViewDetail(string id)
        {
            AccountRepository AccRepo = new AccountRepository();
            OrderRepository orderRepo = new OrderRepository();
            var user = AccRepo.GetAccountById(id);
            var orderHistory = orderRepo.GetOrderByUserId(user.Id);
            ViewBag.Order = orderHistory;
            return View(user);
        }
        [Authorize]

        public IActionResult Ban(string id)
        {
            UserRepository userRepo = new UserRepository();
            userRepo.BanUserById(id);
            return RedirectToAction("Display");
        }

        public IActionResult UserOrderDetail(string OrderId)
        {
            OrderItemRepository orderItemRepo = new OrderItemRepository();
            List<OrderItemModel> orderItemList = orderItemRepo.GetOrderItemByOrderId(OrderId);
            OrderRepository orderRepo = new OrderRepository();
            OrderModel order = orderRepo.GetOrderByOrderId(OrderId);
            order.TotalPrice = 0;
            order.TotalPrice = orderRepo.GetTotalPrice(orderItemList, order);
            ViewBag.UserId = order.UserId;
            ViewBag.Order = order;
            return View(orderItemList);
        }


    }
}

