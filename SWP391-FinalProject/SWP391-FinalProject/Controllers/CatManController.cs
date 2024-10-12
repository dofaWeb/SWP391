using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Controllers
{
    public class CatManController : Controller
    {
        private readonly DBContext db;

        public CatManController(DBContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Display() {
            Repository.CategoryRepository catManRepo = new Repository.CategoryRepository(db);
            var querry=catManRepo.GetAllCategory();
            return View(querry);
        }
        [HttpGet]
        public IActionResult AddCategory()
        {
            
            Repository.CategoryRepository catRepo = new Repository.CategoryRepository(db);

          
            //ViewBag.Category = catRepo.GetAllCategory();
           
            ViewBag.Laptops = catRepo.GetAllCatLaps();
            ViewBag.Smartphones = catRepo.GetAllCatPhone();
            return View();
        }
        [HttpGet]
        public IActionResult EditCategory(string id) {
            Repository.CategoryRepository catManRepo = new Repository.CategoryRepository(db);
            var category=catManRepo.GetCatById(id);
            return View(category);
        }
        [HttpPost]
        public IActionResult AddCategory(string Name, string CategoryType)
        {
            string categoryId = "";
            Repository.CategoryRepository catManRepo = new Repository.CategoryRepository(db);

            // Determine category ID based on selected CategoryType
            if (CategoryType == "laptop")
            {
                categoryId = catManRepo.GenerateCategoryId("B0");
            }
            else if (CategoryType == "phone")
            {
                categoryId = catManRepo.GenerateCategoryId("B1");
            }

            // Create the new category entity
            var newCategory = new Category
            {
                Id = categoryId,
                Name = Name
            };

            // Save the category to the database
            db.Categories.Add(newCategory);
            db.SaveChanges();

            // Redirect to the appropriate view or return success message
            return RedirectToAction("Display");
        }
        [HttpPost]
        public IActionResult EditCategory(string id, string Name)
        {
            var categoryEntity = db.Categories.FirstOrDefault(c => c.Id == id);

            if (categoryEntity == null)
            {
                // Handle case where the category is not found
                return NotFound();
            }

            // Update the entity's name
            categoryEntity.Name = Name;

            // Save the updated entity back to the database
            db.SaveChanges();

            // Redirect to the display page after saving
            return RedirectToAction("Display");
        }
    }
}
