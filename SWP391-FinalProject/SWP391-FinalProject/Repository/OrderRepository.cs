
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;
using Microsoft.Extensions.Primitives;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class OrderRepository
    {
        private readonly DBContext db;



        public OrderRepository()
        {
            db = new DBContext();
        }


        public string NewOrderId()
        {
            string lastId = db.Orders
                .OrderByDescending(r => r.Id)
                .Select(r => r.Id)
                .FirstOrDefault();
            if (lastId == null)
            {
                return "O0000001";
            }
            // Tách phần chữ (A) và phần số (0000001)
            string prefix = lastId.Substring(0, 1); // Lấy ký tự đầu tiên
            int number = int.Parse(lastId.Substring(1)); // Lấy phần số và chuyển thành số nguyên

            // Tăng số lên 1
            int newNumber = number + 1;

            // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }

        public void InsertOrder(OrderModel Order, string username, decimal? TotalPrice, List<ProductItemModel> listProItem)
        {
            int currentHour = DateTime.Now.TimeOfDay.Hours;

            List<StaffShiftModel> staffShifts = (from s in db.StaffShifts
                                                 where s.Date == DateOnly.FromDateTime(DateTime.Today)
                                                 select new StaffShiftModel()
                                                 {
                                                     Id = s.Id,
                                                     Staff_Id = s.StaffId,
                                                 }).ToList();
            if (staffShifts == null || staffShifts.Count==0)
            {
                int preDay = 1;
                do
                {
                    staffShifts = (from s in db.StaffShifts
                                   where s.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(preDay))
                                   select new StaffShiftModel()
                                   {
                                       Id = s.Id,
                                       Staff_Id = s.StaffId,
                                   }).ToList();
                    preDay--;
                } while (staffShifts == null);
            }
            if (currentHour >= 0 && currentHour <= 12)
            {
                Order.StaffShiftId = staffShifts[0].Id;
            }
            else if (currentHour > 12 && currentHour <= 18)
            {
                Order.StaffShiftId = staffShifts[1].Id;
            }
            else
            {
                staffShifts = (from s in db.StaffShifts
                               where s.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(1))
                               select new StaffShiftModel()
                               {
                                   Staff_Id = s.StaffId,
                                   Id = s.Id
                               }).ToList();

                Order.StaffShiftId = staffShifts[0].Id;
            }

            string newId = NewOrderId();

            var newOrder = new Entities.Order()
            {
                Id = newId,
                UserId = Order.UserId,
                Address = Order.Addres,
                StateId = 1,
                Date = DateTime.Now,
                UsePoint = (decimal)(Order.UsePoint ?? Order.UsePoint),
                EarnPoint = TotalPrice / 1000,
                StaffShiftId = Order.StaffShiftId
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();

            InsertOrderItem(listProItem, newOrder.Id);
            UserRepository userRepo = new UserRepository();
            userRepo.UpdateUserPoint(username, newOrder.StateId, newOrder.UsePoint, newOrder.EarnPoint);
        }

        public void InsertOrderItem(List<ProductItemModel> listProItem, string orderID)
        {
            foreach (ProductItemModel item in listProItem)
            {
                var newItem = new Entities.OrderItem()
                {
                    OrderId = orderID,
                    ProductItemId = item.Id,
                    Quantity = item.CartQuantity,
                    Discount = item.Discount,
                    Price = item.SellingPrice ?? 0
                };
                db.OrderItems.Add(newItem);
                db.SaveChanges();
                ProductItemRepository proItemRepo = new ProductItemRepository();
                proItemRepo.UpdateProductItemQuantityByOrderStateId(newItem.ProductItemId, newItem.Quantity, 1);
            }
        }


        public List<OrderModel> GetAllStaffOrder(string staffId)
        {
            var query = from o in db.Orders

                        join u in db.Users on o.UserId equals u.Account.Id
                        join ot in db.OrderStates on o.StateId equals ot.Id
                        join ss in db.StaffShifts on o.StaffShiftId equals ss.Id
                        where ss.StaffId == staffId
                        select new OrderModel
                        {
                            Id = o.Id,
                            UserId = o.UserId,
                            Addres = o.Address,
                            StateId = o.StateId,
                            Date = o.Date,
                            UsePoint = o.UsePoint,
                            EarnPoint = o.EarnPoint ?? 0,
                            StaffShiftId = o.StaffShiftId,

                            //Quantity = oi.Quantity,
                            //Price = oi.Price,
                            //Discount = oi.Discount,
                            User = new UserModel() { Name = u.Name },

                            OrderState = new OrderState() { Name = ot.Name }

                        };
            var result = query.ToList();
            return result;
        }
        public List<OrderModel> GetAllOrder()
        {
            var query = from o in db.Orders

                        join u in db.Users on o.UserId equals u.Account.Id
                        join ot in db.OrderStates on o.StateId equals ot.Id
                        join ss in db.StaffShifts on o.StaffShiftId equals ss.Id
                        join s in db.Staff on ss.StaffId equals s.AccountId

                        select new OrderModel
                        {
                            Id = o.Id,
                            UserId = o.UserId,
                            Addres = o.Address,
                            StateId = o.StateId,
                            Date = o.Date,
                            UsePoint = o.UsePoint,
                            EarnPoint = o.EarnPoint ?? 0,
                            StaffShiftId = o.StaffShiftId,
                            StaffName = s.Name,

                            //Quantity = oi.Quantity,
                            //Price = oi.Price,
                            //Discount = oi.Discount,
                            User = new UserModel() { Name = u.Name },

                            OrderState = new OrderState() { Name = ot.Name }

                        };
            var result = query.ToList();
            return result;
        }

        public OrderModel GetOrderByOrderId(string OrderId)
        {
            var result = from o in db.Orders
                         join os in db.OrderStates on o.StateId equals os.Id
                         where o.Id == OrderId
                         select new OrderModel()
                         {
                             Id = o.Id,
                             UserId = o.UserId,
                             Addres = o.Address,
                             StateId = o.StateId,
                             OrderState = new OrderState() { Name = os.Name },
                             UsePoint = o.UsePoint,
                             EarnPoint = o.EarnPoint ?? 0,
                             Date = o.Date
                         };

            OrderModel order = result.FirstOrDefault();
            return order;
        }

        public List<OrderModel> GetOrderByUserId(string UserId)
        {
            string query = "Select o.id as OrderId, o.user_id as UserId, o.address as Address, " +
               "o.state_id as StateId, os.name as StateName, o.use_point as UsePoint, " +
               "o.earn_point as EarnPoint, o.date as OrderDate " +
               "From `Order` o " +
               "Inner join Order_State os on o.state_id = os.id " +
               "Where o.user_id = @UserId";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId }
            };
            DataTable orderTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            List<OrderModel> orderList = new List<OrderModel>();
            foreach (DataRow row in orderTable.Rows)
            {
                orderList.Add(new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["UserId"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = int.Parse(row["StateId"].ToString()),
                    OrderState = new OrderState { Name = row["StateName"].ToString() },
                    UsePoint = decimal.Parse(row["UsePoint"].ToString()),
                    EarnPoint = decimal.Parse(row["EarnPoint"].ToString()),
                    Date = DateTime.Parse(row["OrderDate"].ToString())
                });
            }

            OrderItemRepository orderItemRepo = new OrderItemRepository();
            List<OrderItemModel> orderItemList = new List<OrderItemModel>();

            foreach (var order in orderList)
            {
                orderItemList = orderItemRepo.GetOrderItemByOrderId(order.Id);
                order.TotalPrice = 0;
                order.TotalPrice = GetTotalPrice(orderItemList, order);
            }
            return orderList;
        }

        public List<OrderState> GetAllOrderState()
        {
            var query = db.OrderStates.Select(p => new OrderState
            {
                Id = p.Id,
                Name = p.Name,
            }).ToList();
            return query;
        }

        public void UpdateOrderState(int orderStateId, string OrderId)
        {
            var order = db.Orders.Where(p => p.Id == OrderId).FirstOrDefault();
            order.StateId = orderStateId;
            db.SaveChanges();

        }

        public List<OrderItemModel> GetOrderItemByOrderId(string OrderId)
        {
            ProductRepository proRepo = new ProductRepository();
            var query = from oi in db.OrderItems
                        join pi in db.ProductItems on oi.ProductItemId equals pi.Id
                        join p in db.Products on pi.ProductId equals p.Id
                        join o in db.Orders on oi.OrderId equals o.Id
                        where o.Id == OrderId
                        select new OrderItemModel
                        {
                            Product = new ProductModel
                            {
                                Picture = p.Picture,
                                Name = p.Name
                            },
                            Ram = proRepo.GetProductVariationOption(pi.Id, "Ram"),
                            Storage = proRepo.GetProductVariationOption(pi.Id, "Storage"),
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Discount = pi.Discount,

                            // Corrected discount calculation
                            TotalPrice = (oi.Quantity * oi.Price) * (1 - ((pi.Discount ?? 0) / 100)),
                        };
            return query.ToList();
        }


        public decimal? GetTotalPrice(List<OrderItemModel> orderItems, OrderModel order)
        {
            foreach (var item in orderItems)
            {
                // If Discount is null, it will default to 0
                decimal discount = item.Discount ?? 0;

                // Calculate total price for each item and add it to order.TotalPrice
                order.TotalPrice += (item.Price * item.Quantity) * (1 - (discount / 100));
            }
            return order.TotalPrice;
        }


        public OrderModel GetOrderDetail(string OrderId)
        {
            var order = db.Orders.Where(p => p.Id == OrderId).Select(p => new OrderModel
            {
                EarnPoint = p.EarnPoint,
                UsePoint = p.UsePoint,
                TotalPrice = 0
            }).FirstOrDefault();

            return order;
        }
    }
}
