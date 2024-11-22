using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class Order : Controller
    {
        [HttpPost]
        public IActionResult Checkout(int Point, string Username, string Province, string District, string Address)
        {
            OrderModel order = new OrderModel();
            OrderRepository orderRepo = new OrderRepository();
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(Username);
            order.UserId = user.Account.Id;
            order.UsePoint = Point;
            order.Addres = Province + " ," + District + " ," + Address;
            string Resultcookie = Request.Cookies["CartCookie"] ?? "";
            string[] tmp = Resultcookie.Split('=');
            int sizeOfCookie = tmp.Length;
            decimal? TotalPrice =0;
            List<ProductItemModel> listProItem = new List<ProductItemModel>();
            for (int i = 1; i < sizeOfCookie; i++)
            {
                string[] eachCookie = tmp[i].Split('/');
                ProductItemModel item = new ProductItemModel();
                item.Id = eachCookie[0];
                item.Product = new ProductModel();
                item.Product.Id = eachCookie[1];
                item.Product.Name = eachCookie[2];
                item.Product.Picture = eachCookie[3];
                item.Product.Description = eachCookie[4];
                item.CartQuantity = int.Parse(eachCookie[5]);
                item.Quantity = int.Parse(eachCookie[6]);
                item.SellingPrice = Decimal.Parse(eachCookie[7]);
                item.Discount = Decimal.Parse(eachCookie[8]);
                item.PriceAfterDiscount = Decimal.Parse(eachCookie[9]);
                TotalPrice += item.PriceAfterDiscount;
                item.Ram = eachCookie[10];
                item.Storage = eachCookie[11];
                listProItem.Add(item);
            }
            orderRepo.InsertOrder(order, Username, TotalPrice, listProItem);
            Helpers.MailUtil.SendBillEmail(order, Username, TotalPrice, Point, listProItem);
            Response.Cookies.Delete("CartCookie");
            return RedirectToAction("Index", "Pro");
        }

        public async Task<IActionResult> ProcessCheckout() {
            string CartCookies = Request.Cookies["CartCookie"];
            if(CartCookies==null || CartCookies.Equals(""))
            {
                // Ghi log khi không có gì trong giỏ hàng
                return RedirectToAction("Index", "Cart");
            }

            string username = Request.Cookies["Username"];
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(username);
            ViewBag.User = user;
            //-----------------------------------------

            string[] tmp = CartCookies.Split('=');
            int sizeOfCookie = tmp.Length;
            List<ProductItemModel> listProItem = new List<ProductItemModel>();
            for (int i = 1; i < sizeOfCookie; i++)
            {
                string[] eachCookie = tmp[i].Split('/');
                ProductItemModel item = new ProductItemModel();
                item.Id = eachCookie[0];
                item.Product = new ProductModel();
                item.Product.Id = eachCookie[1];
                item.Product.Name = eachCookie[2];
                item.Product.Picture = eachCookie[3];
                item.Product.Description = eachCookie[4];
                item.CartQuantity = int.Parse(eachCookie[5]);
                item.Quantity = int.Parse(eachCookie[6]);
                item.SellingPrice = Decimal.Parse(eachCookie[7]);
                item.Discount = Decimal.Parse(eachCookie[8]);
                item.PriceAfterDiscount = Decimal.Parse(eachCookie[9]);
                item.Ram = eachCookie[10];
                item.Storage = eachCookie[11];
                listProItem.Add(item);
            }

            ViewBag.ListProItem = listProItem;
            //-----------------------------------------

            HttpClient client = new HttpClient();
            try
            {
                string url = "https://esgoo.net/api-tinhthanh/1/0.htm";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseJson);
                    int error = jsonResponse.error;

                    if (error == 0)
                    {
                        // Get the "data" array from the JSON response
                        var data = jsonResponse.data;

                        // Pass the data to the view using ViewBag
                        ViewBag.Provinces = data;

                        return View();
                    }
                    else
                    {
                        // Handle the error from the API
                        ViewBag.Error = jsonResponse.error_text;
                        //return View("Register");
                    }
                }
                else
                {
                    // Handle non-success HTTP response
                    ViewBag.Error = "Error occurred while sending the request to the API: " + (int)response.StatusCode;
                    //return View("Register");
                }
            }
            catch (Exception e)
            {
                // Handle any exceptions
                ViewBag.Error = "An error occurred while processing the request: " + e.Message;
                //return View("Register");
            }
            finally
            {
                client.Dispose();
            }
            return View();
        }

        public IActionResult UserOrderHistory(string UserId)
        {
            OrderRepository orderRepo = new OrderRepository();
            List<OrderModel> orderList = orderRepo.GetOrderByUserId(UserId);
            return View(orderList);
        }

        public IActionResult Cancel(string id, string user_id)
        {
            OrderRepository orderRepo = new OrderRepository();
            orderRepo.Cancel(id);
            return RedirectToAction("UserOrderHistory", new { UserId = user_id });
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
