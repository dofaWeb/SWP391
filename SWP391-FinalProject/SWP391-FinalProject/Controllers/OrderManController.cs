using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Security.Claims;


namespace SWP391_FinalProject.Controllers
{
    public class OrderManController : Controller
    {
        

        public IActionResult ListOrder(string Username)
        {
            AccountRepository accountRepository = new AccountRepository();
            var Id = accountRepository.GetIdByUsername(Username);
            Repository.OrderRepository orderRepo = new Repository.OrderRepository();
            List<OrderModel> orderList;
            if (Id == "A0000001")
            {
                orderList = orderRepo.GetAllOrder();
            }
            else
            {
                orderList = orderRepo.GetAllStaffOrder(Id);
            }
            List<OrderState> orderStates = orderRepo.GetAllOrderState();
            ViewBag.OrderState = orderStates;
            return View(orderList);
        }

        public IActionResult UpdateState(int OrderStateId, string OrderId)
        {
            OrderRepository orderRepository = new OrderRepository();
            orderRepository.UpdateOrderState(OrderStateId, OrderId);

            var username = User.FindFirst(ClaimTypes.Name);
            UserRepository userRepo = new UserRepository();
            OrderModel order = orderRepository.GetOrderByOrderId(OrderId);
            UserModel user = userRepo.GetUserProfileByUserId(order.UserId);
            userRepo.UpdateUserPoint(user.Account.Username, OrderStateId, order.UsePoint, order.EarnPoint);

            return RedirectToAction("ListOrder", new {Username = username});
        }

        public IActionResult History()
        {
            Repository.OrderRepository orderRepo = new OrderRepository();
            var OrderHis = orderRepo.GetAllOrder();
            return View(OrderHis);
        }
       
        public IActionResult OrderDetail(string OrderId)
        {
            OrderRepository orderRepository = new OrderRepository();
            var orderItems = orderRepository.GetOrderItemByOrderId(OrderId);
            var orderDetail = orderRepository.GetOrderDetail(OrderId);
            var TotalPrice = orderRepository.GetTotalPrice(orderItems, orderDetail);
            ViewBag.OrderDetail = orderDetail;
            ViewBag.TotalPrice = TotalPrice;
            return View(orderItems);
        }
    }
}
