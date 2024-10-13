﻿using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class ProductItemRepository
    {
        private readonly DBContext db;
        public ProductItemRepository(DBContext _context)
        {
            db = _context;
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

        public Boolean checkExistVariation(string proItemId, string variationOpId1, string variationOpId2)
        {
            var v1 = db.ProductConfigurations.Where(p => p.ProductItemId == proItemId && p.VariationOptionId == variationOpId1).Any();
            var v2 = db.ProductConfigurations.Where(p => p.ProductItemId == proItemId && p.VariationOptionId == variationOpId2).Any();
            if (v1 && v2)
            {
                return false;
            }
            return true;
        }

        public void AddProductConfiguration(ProductItemModel model)
        {
            var variation_option_id1 = GetVariationOptionId(model.Ram, GetVariationId("Ram"));
            var variation_option_id2 = GetVariationOptionId(model.Storage, GetVariationId("Storage"));

            if (checkExistVariation(model.Id, variation_option_id1, variation_option_id2))
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
    }
}