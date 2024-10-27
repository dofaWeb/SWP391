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

    }
}
