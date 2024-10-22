
﻿using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

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

            if(currentHour >= 0 && currentHour <= 12)
            {
                Order.StaffShiftId = staffShifts[0].Id;
            }
            else if(currentHour>12 && currentHour<=18)
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
                Date = DateTime.Today,
                UsePoint = Order.UsePoint,
                EarnPoint = TotalPrice / 1000,
                StaffShiftId = Order.StaffShiftId
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();

            InsertOrderItem(listProItem, newOrder.Id);
            UpdateUserPoint(username, newOrder.UsePoint);
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
            }
        }
        
        void UpdateUserPoint(string username, decimal? UserPoint)
        {
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(username);
            user.Point -= UserPoint.HasValue ? (int)UserPoint.Value : 0;
            userRepo.UpdateUser(user);
            db.SaveChanges();
        }
    }
}
