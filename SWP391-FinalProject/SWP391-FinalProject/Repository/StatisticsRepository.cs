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
        public List<StatisticsModel> GetImportPrice()
        {
            var query = from ql in db.QuantityLogs
                        join pi in db.ProductItems
                        on ql.ProductItemId equals pi.Id
                        where ql.NewQuantity > ql.OldQuantity
                        group new { ql, pi.ImportPrice } by new { ql.ChangeTimestamp.Year, ql.ChangeTimestamp.Month, ql.ProductItemId } into g
                        orderby g.Key.Year, g.Key.Month
                        select new StatisticsModel
                        {
                            ChangeDate = new DateTime(g.Key.Year, g.Key.Month, 1),
                            ProductItemId = g.Key.ProductItemId,
                            TotalQuantityAdded = g.Sum(x => x.ql.NewQuantity - x.ql.OldQuantity),
                            TotalImportPrice = g.Sum(x => (x.ql.NewQuantity - x.ql.OldQuantity) * x.ImportPrice)
                        };


            return query.ToList();
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
