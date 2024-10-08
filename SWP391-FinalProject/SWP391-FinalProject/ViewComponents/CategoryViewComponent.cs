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

        public IViewComponentResult Invoke()
        {
            var data = db.Categories.Select(l => new Category
            {
                Name = l.Name,
            });
            return View(data);
        }
    }
}
