using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            string Resultcookie = getCartFromCookie();
            string[] tmp = Resultcookie.Split('=');
            int sizeOfCookie = tmp.Length;
            List<ProductItemModel> listProItem = new List<ProductItemModel>();
            decimal? TotalPrice = 0;
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
            ViewBag.TotalPrice = TotalPrice;
            return View(listProItem);
        }

        public IActionResult AddToCart(string Option, string ProductId)
        {
            var parts = Option.Split(new string[] { "RAM: ", "<br/> Storage: " }, StringSplitOptions.None);


            string ram = parts[1];
            string storage = parts[2];
            ProductRepository productRepository = new ProductRepository();
            string ProductItemId = productRepository.GetProItemIdByVariation(ram, storage, ProductId);
            ProductItemRepository proItemRepo = new ProductItemRepository();
            ProductItemModel proItem = proItemRepo.getProductItemByProductItemId(ProductItemId);
            proItem.Ram = ram;
            proItem.Storage = storage;
            proItem.PriceAfterDiscount = ProductRepository.CalculatePriceAfterDiscount(proItem.SellingPrice, proItem.Discount / 100);
            AddToCartCookie(proItem);
            TempData["AddCartSuccess"] = "Added to the cart successfully";
            return RedirectToAction("ProductDetail", "Pro", new { id = proItem.Product.Id, productItemId = ProductItemId, Price = proItemRepo.GetPriceByProductItemId(ProductItemId) });
        }

        public void AddToCartCookie(ProductItemModel proItem)
        {
            string Resultcookie = getCartFromCookie();
            CookieOptions Cookie = new CookieOptions();
            Cookie.Expires = DateTime.Now.AddDays(1);
            string newResultCookie = "";
            if (Resultcookie == "")
            {
                string NewCookieData = "=" + proItem.Id + "/" + proItem.Product.Id + "/" + proItem.Product.Name + "/" + proItem.Product.Picture + "/" + proItem.Product.Description + "/" + "1" + "/" + proItem.Quantity + "/" + proItem.SellingPrice + "/" + proItem.Discount + "/" + proItem.PriceAfterDiscount + "/" + proItem.Ram + "/" + proItem.Storage;
                Response.Cookies.Append("CartCookie", NewCookieData, Cookie);
            }
            else
            {
                string[] tmp = Resultcookie.Split('=');
                int sizeOfCookie = tmp.Length;
                bool isExist = false;

                for (int i = 0; i < sizeOfCookie; i++)
                {
                    string[] eachCookie = tmp[i].Split('/');
                    if (eachCookie[0] == proItem.Id)
                    {
                        if (int.Parse(eachCookie[5]) + 1 <= int.Parse(eachCookie[6]) && int.Parse(eachCookie[5]) < 5)
                        {
                            int newQuantity = int.Parse(eachCookie[5]) + 1;
                            eachCookie[5] = newQuantity + "";
                            string alternativeCookie = "";
                            decimal newPrice = ProductRepository.CalculatePriceAfterDiscount(decimal.Parse(eachCookie[7]), decimal.Parse(eachCookie[8]) / 100) * newQuantity;
                            eachCookie[9] = newPrice + "";
                            for (int j = 0; j < eachCookie.Length - 1; j++)
                            {
                                alternativeCookie += eachCookie[j] + "/";
                            }
                            alternativeCookie += eachCookie[eachCookie.Length - 1];
                            tmp[i] = alternativeCookie;
                            isExist = true;
                        }
                        else
                        {
                            isExist = true;
                        }
                        break;
                    }
                }

                if (isExist)
                {
                    foreach (string eachCookie in tmp)
                    {
                        newResultCookie += "=" + eachCookie;
                    }
                    newResultCookie = newResultCookie.Substring(1);
                }
                if (!isExist)
                {
                    string NewCookieData = "=" + proItem.Id + "/" + proItem.Product.Id + "/" + proItem.Product.Name + "/" + proItem.Product.Picture + "/" + proItem.Product.Description + "/" + "1" + "/" + proItem.Quantity + "/" + proItem.SellingPrice + "/" + proItem.Discount + "/" + proItem.PriceAfterDiscount + "/" + proItem.Ram + "/" + proItem.Storage;
                    newResultCookie = Resultcookie;
                    newResultCookie += NewCookieData;
                }
                Response.Cookies.Append("CartCookie", newResultCookie, Cookie);
            }
        }

        public string getCartFromCookie()
        {
            string cookie = "";
            if (Request?.Cookies != null && Request.Cookies["CartCookie"] != null)
            {
                cookie = Request.Cookies["CartCookie"];
            }

            return cookie;
        }

        [HttpPost]
        public IActionResult Quantity(string ProductItemId, string Action)
        {
            string Resultcookie = getCartFromCookie();
            CookieOptions Cookie = new CookieOptions();
            Cookie.Expires = DateTime.Now.AddDays(1);
            string[] tmp = Resultcookie.Split('=');
            int sizeOfCookie = tmp.Length;
            string newResultCookie = "";
            for (int i = 0; i < sizeOfCookie; i++)
            {
                string[] eachCookie = tmp[i].Split('/');
                if (eachCookie[0] == ProductItemId)
                {
                    if (Action.Equals("increase"))
                    {
                        if (int.Parse(eachCookie[5]) + 1 <= int.Parse(eachCookie[6]))
                        {
                            int newQuantity = int.Parse(eachCookie[5]) + 1;
                            eachCookie[5] = newQuantity + "";
                            decimal newPrice = ProductRepository.CalculatePriceAfterDiscount(decimal.Parse(eachCookie[7]), decimal.Parse(eachCookie[8]) / 100) * newQuantity;
                            eachCookie[9] = newPrice + "";
                            string alternativeCookie = "";
                            for (int j = 0; j < eachCookie.Length - 1; j++)
                            {
                                alternativeCookie += eachCookie[j] + "/";
                            }
                            alternativeCookie += eachCookie[eachCookie.Length - 1];
                            tmp[i] = alternativeCookie;
                        }
                        foreach (string each in tmp)
                        {
                            newResultCookie += "=" + each;
                        }
                        newResultCookie = newResultCookie.Substring(1);
                        break;
                    }
                    else if (Action.Equals("decrease"))
                    {
                        if (int.Parse(eachCookie[5]) - 1 > 0)
                        {
                            int newQuantity = int.Parse(eachCookie[5]) - 1;
                            eachCookie[5] = newQuantity + "";
                            string alternativeCookie = "";
                            decimal newPrice = ProductRepository.CalculatePriceAfterDiscount(decimal.Parse(eachCookie[7]), decimal.Parse(eachCookie[8]) / 100) * newQuantity;
                            eachCookie[9] = newPrice + "";
                            for (int j = 0; j < eachCookie.Length - 1; j++)
                            {
                                alternativeCookie += eachCookie[j] + "/";
                            }
                            alternativeCookie += eachCookie[eachCookie.Length - 1];
                            tmp[i] = alternativeCookie;
                        }
                        foreach (string each in tmp)
                        {
                            newResultCookie += "=" + each;
                        }
                        newResultCookie = newResultCookie.Substring(1);
                        break;
                    }
                    else if (Action.Equals("remove"))
                    {
                        foreach (string each in tmp)
                        {
                            if (!tmp[i].Equals(each))
                            {
                                newResultCookie += "=" + each;
                            }
                        }
                        newResultCookie = newResultCookie.Substring(1);
                    }
                    break;
                }
            }
            Response.Cookies.Append("CartCookie", newResultCookie, Cookie);
            return RedirectToAction("Index");
        }
    }
}
