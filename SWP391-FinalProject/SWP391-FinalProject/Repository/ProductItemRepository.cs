using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class ProductItemRepository
    {
        private readonly DBContext db;
        public ProductItemRepository()
        {
            db = new DBContext();
        }

        public string getNewProductItemID()
        {
            string lastId = db.ProductItems
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "Pi000001";
            }
            // Tách phần chữ (A) và phần số (0000001)
            string prefix = lastId.Substring(0, 2); // Lấy ký tự đầu tiên
            int number = int.Parse(lastId.Substring(2)); // Lấy phần số và chuyển thành số nguyên

            // Tăng số lên 1
            int newNumber = number + 1;

            // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
            string newId = $"{prefix}{newNumber:D6}";

            return newId;
        }

        public int getNewProducConfigurationID()
        {
            int lastId = db.ProductConfigurations
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return 1;
            }
            lastId++;
            return lastId;
        }

        public string GetVariationId(string name)
        {
            var id = db.Variations.Where(p=>p.Name == name).Select(p => p.Id).FirstOrDefault();
            return id;
        }

        public string GetVariationOptionId(string value, string varationId)
        {
            var id = db.VariationOptions.Where(p=>p.Value == value && p.VariationId==varationId).Select(p => p.Id).FirstOrDefault();
            return id;
        }

        public Boolean checkExistVariation(string productId, string variationOpId1, string variationOpId2)
        {
            var v1 = (from p in db.Products
                      join pi in db.ProductItems on p.Id equals pi.ProductId
                      join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId
                      where p.Id == productId && pc.VariationOptionId == variationOpId1
                      select p).Any();
            var v2 = (from p in db.Products
                      join pi in db.ProductItems on p.Id equals pi.ProductId
                      join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId
                      where p.Id == productId && pc.VariationOptionId == variationOpId2
                      select p).Any();
            if (v1 && v2)
            {
                return true;
            }
            return false;
        }

        public ProductItemModel getProductItemByProductItemId(string proItemId)
        {
            var proItem = from pi in db.ProductItems
                          join p in db.Products on pi.ProductId equals p.Id
                          where pi.Id == proItemId
                          select new ProductItemModel
                          {
                              Id = pi.Id,
                              SellingPrice = pi.SellingPrice,
                              Quantity = pi.Quantity,
                              Discount = pi.Discount,
                              Product = new ProductModel
                              {
                                  Name = p.Name,
                                  Picture = p.Picture,
                                  Description = p.Description,
                                  Id = p.Id
                              }
                          };
            var result = proItem.FirstOrDefault();
            return result;
        }

        public void AddProductConfiguration(ProductItemModel model)
        {
            var variation_option_id1 = GetVariationOptionId(model.Ram, GetVariationId("Ram"));
            var variation_option_id2 = GetVariationOptionId(model.Storage, GetVariationId("Storage"));

            if (checkExistVariation(model.ProductId, variation_option_id1, variation_option_id2))
            {
                // Retrieve the existing ProductItem from the database
                var existingProductItem = db.ProductItems.FirstOrDefault(p => p.Id == model.Id);

                if (existingProductItem != null)
                {
                    // Remove the existing entity
                    db.ProductItems.Remove(existingProductItem);
                    db.SaveChanges();
                }

                return;
            }
            else
            {
                var id = getNewProducConfigurationID();

                db.ProductConfigurations.Add(new Entities.ProductConfiguration
                {
                    Id = id,
                    ProductItemId = model.Id,
                    VariationOptionId = variation_option_id1,
                });
                db.SaveChanges();

                id++;
                db.ProductConfigurations.Add(new Entities.ProductConfiguration
                {
                    Id = id,
                    ProductItemId = model.Id,
                    VariationOptionId = variation_option_id2,
                });
                db.SaveChanges();
            }
        }


        public void AddProductItem(ProductItemModel model)
        {
            model.Id = getNewProductItemID();
            model.Discount = 0;

            db.ProductItems.Add(new Entities.ProductItem
            {
                Id = model.Id,
                ProductId = model.ProductId,
                Quantity = model.Quantity,
                ImportPrice = model.ImportPrice,
                SellingPrice = model.SellingPrice,
                Discount = model.Discount,
            });
            var product = db.Products.Where(p => p.Id == model.ProductId && p.StateId != 2).FirstOrDefault();
            product.StateId = 1;
            db.SaveChanges();
            AddProductConfiguration(model);
        }

        public void EditProductItem(ProductItemModel model)
        {
            var proItem = db.ProductItems.FirstOrDefault(db => db.Id == model.Id);
            if (proItem != null)
            {
                proItem.SellingPrice = model.SellingPrice;
                proItem.Discount = model.Discount;
                db.SaveChanges();
            }
        }

        public void Delete(string id)
        {
            var proItem = db.ProductItems.FirstOrDefault(db => db.Id == id);
            var prc = db.ProductConfigurations.Where(p => p.ProductItemId == id).ToList();
            if (proItem != null)
            {
                db.ProductItems.Remove(proItem);
                foreach(var pc in prc)
                {
                    db.ProductConfigurations.Remove(pc);
                }
                db.SaveChanges();
                ProductRepository proRepo = new ProductRepository();
                proRepo.UpdateProductState(proItem.ProductId);
            }
        }

        public decimal? GetPriceByProductItemId(string productItemId)
        {
            var price = db.ProductItems.Where(p => p.Id == productItemId).Select(p => new
            {
                SellingPrice = p.SellingPrice,
                Discount = p.Discount
            }).FirstOrDefault();

            return ProductRepository.CalculatePriceAfterDiscount(price.SellingPrice, price.Discount/100);
        }

        public void Import(string id, int quantity)
        {
            var proItem = db.ProductItems.FirstOrDefault(db => db.Id == id);
            if(proItem != null)
            {
                proItem.Quantity += quantity;
                db.SaveChanges();
            }
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProductState(proItem.ProductId);
        }
    }
}
