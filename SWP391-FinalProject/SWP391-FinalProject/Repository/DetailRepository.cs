using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class DetailRepository
    {
        DBContext db;
        public DetailRepository()
        {
            db = new DBContext();
        }


        public ProductItemModel GetProductById(string productId)
        {
            // Truy vấn dữ liệu bằng LINQ
            var productItem = (from p in db.Products
                               join pi in db.ProductItems on p.Id equals pi.ProductId
                               join v in db.VariationOptions on pi.Id equals v.Id // Nếu có bảng Variations
                               where p.Id == productId
                               select new ProductItemModel
                               {
                                   Id = pi.Id,
                                   ProductId = pi.ProductId,
                                   Product = new ProductModel
                                   {
                                       Id = p.Id,
                                       Name = p.Name,
                                       Picture = p.Picture,
                                       Description = p.Description,
                                       CategoryId = p.CategoryId,
                                       CategoryName = db.Categories
                                                   .Where(c => c.Id == p.CategoryId)
                                                   .Select(c => c.Name).FirstOrDefault(), // Lấy tên danh mục

                                       StateId = p.StateId
                                   },
                                   Quantity = pi.Quantity,
                                   ImportPrice = pi.ImportPrice,
                                   SellingPrice = pi.SellingPrice,
                                   Discount = pi.Discount,

                               }).FirstOrDefault();

            return productItem;

        }

    }
}
