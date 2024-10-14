using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace SWP391_FinalProject.ViewComponents
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly DBContext db;

        public CategoryViewComponent(DBContext context) => db = context;

        
        public IViewComponentResult Invoke(string CatType)
        {
            Repository.CategoryRepository catRepo = new Repository.CategoryRepository();
            List<Models.CategoryModel> category;
            switch (CatType)
            {
                case "Laptops":
                    category = catRepo.GetAllCatLaps();
                    break;
                case "Phones":
                    category = catRepo.GetAllCatPhone();
                    break;
                 default:
                    category = catRepo.GetAllCategory();
                    break;
            }
            
            return View(category);
        }
    }
}
