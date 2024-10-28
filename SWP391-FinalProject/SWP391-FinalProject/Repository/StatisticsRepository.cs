using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class StatisticsRepository
    {
        private readonly DBContext db;
        public StatisticsRepository()
        {
            db = new DBContext();
        }
        public dynamic GetSellingPrice(int year)
        {
            var query = db.Orders
    .Where(o => o.StateId == 2 && o.Date.Year == year)
    .Join(db.OrderItems,
          o => o.Id,
          oi => oi.OrderId,
          (o, oi) => new { o, oi })
    .GroupBy(x => x.o.Date.Month)
    .Select(g => new
    {
        Month = g.Key,
        TotalSellingPrice = g.Sum(x => x.oi.Quantity * x.oi.Price * (1 - x.oi.Discount / 100))
    })
    .OrderBy(result => result.Month)
    .ToList();


            return query;
        }

        public dynamic GetImportPrice(int year)
        {
            var result = db.QuantityLogs
    .Where(ql => ql.NewQuantity > ql.OldQuantity && ql.ChangeTimestamp.Year == 2024)
    .GroupBy(ql => ql.ChangeTimestamp.Month)
    .Select(g => new
    {
        ChangeMonth = g.Key,
        TotalImportPrice = g.Sum(ql => (ql.NewQuantity - ql.OldQuantity) * ql.ProductItem.ImportPrice)
    })
    .OrderBy(x => x.ChangeMonth)
    .ToList();

            return result;
        }

        public dynamic GetOrderStat()
        {
            var result = db.Orders
                        .Where(o => o.StateId == 2) // Optional: Filter approved orders
                        .GroupBy(o => new { Year = o.Date.Year, Month = o.Date.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            TotalOrder = g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity))
                        })
                        .OrderBy(r => r.Year)
                        .ThenBy(r => r.Month)
                        .ToList();

            return result;
        }

        public dynamic GetBestSellingProducts()
        {
            var topProducts = (from o in db.Orders
                               join oi in db.OrderItems on o.Id equals oi.OrderId
                               join pi in db.ProductItems on oi.ProductItemId equals pi.Id
                               join p in db.Products on pi.ProductId equals p.Id
                               where o.StateId == 2
                               group pi by p.Name into productGroup
                               select new
                               {
                                   ProductName = productGroup.Key,
                                   Count = productGroup.Count()
                               })
                   .OrderByDescending(x => x.Count)
                   .Take(5)
                   .ToList();
            return topProducts;
        }

        public dynamic GetMostProfitableProducts()
        {
            var topProfitableProducts = (from o in db.Orders
                                         join oi in db.OrderItems on o.Id equals oi.OrderId
                                         join pi in db.ProductItems on oi.ProductItemId equals pi.Id
                                         join p in db.Products on pi.ProductId equals p.Id
                                         where o.StateId == 2
                                         group new { oi, pi } by p.Name into productGroup
                                         let totalRevenue = productGroup.Sum(x => x.oi.Quantity * x.oi.Price * (1 - x.oi.Discount / 100))
                                         let totalImportCost = productGroup.Sum(x => x.pi.ImportPrice) // Changed from Count() to Sum()
                                         select new
                                         {
                                             ProductName = productGroup.Key,
                                             Profit = totalRevenue - totalImportCost
                                         })
                 .OrderByDescending(x => x.Profit)
                 .Take(5)
                 .ToList();

            return topProfitableProducts;
        }

        public dynamic GetBestSellingBrands()
        {
            var topCategories = (from o in db.Orders
                                 join oi in db.OrderItems on o.Id equals oi.OrderId
                                 join pi in db.ProductItems on oi.ProductItemId equals pi.Id
                                 join p in db.Products on pi.ProductId equals p.Id
                                 join c in db.Categories on p.CategoryId equals c.Id
                                 where o.StateId == 2
                                 group pi by c.Name into categoryGroup
                                 select new
                                 {
                                     CategoryName = categoryGroup.Key,
                                     ProductCount = categoryGroup.Count()
                                 })
                     .OrderByDescending(x => x.ProductCount)
                     .Take(5)
                     .ToList();
            return topCategories;
        }

        public dynamic GetMostSpendingCustomers()
        {
            var topSpendingUsers = (from o in db.Orders
                                    join oi in db.OrderItems on o.Id equals oi.OrderId
                                    join u in db.Users on o.UserId equals u.AccountId
                                    where o.StateId == 2
                                    group oi by u.Name into userGroup
                                    select new
                                    {
                                        UserName = userGroup.Key,
                                        MoneyUsed = userGroup.Sum(x => x.Quantity * x.Price * (1 - x.Discount / 100))
                                    })
                        .OrderByDescending(x => x.MoneyUsed)
                        .Take(5)
                        .ToList();


            return topSpendingUsers;
        }

    }
}
