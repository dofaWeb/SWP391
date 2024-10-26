using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;


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
            return RedirectToAction("ListOrder");
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
