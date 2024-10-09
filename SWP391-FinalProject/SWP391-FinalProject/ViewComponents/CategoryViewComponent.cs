using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using System.Reflection.Metadata.Ecma335;

namespace SWP391_FinalProject.ViewComponents
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly DBContext db;

        public CategoryViewComponent(DBContext context) => db = context;

        
        public IViewComponentResult Invoke(string CatType)
        {
            Repository.Category catRepo = new Repository.Category(db);
            List<Models.CategoryModel> category;
            if (CatType == "Laptops")
            {
                category=catRepo.GetAllCatLaps();
            }
            else if(CatType =="Phones")
            {
                category = catRepo.GetAllCatPhone();
            }
            else
            {
                category = catRepo.GetAllCategory();
            }
            return View(category);
        }
    }
}
