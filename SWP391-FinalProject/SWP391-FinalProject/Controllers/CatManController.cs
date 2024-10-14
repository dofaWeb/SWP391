using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class CatManController : Controller
    {
        public CatManController()
        {

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Display() {
            Repository.CategoryRepository catManRepo = new Repository.CategoryRepository();
            var querry=catManRepo.GetAllCategory();
            return View(querry);
        }
        [HttpGet]
        public IActionResult AddCategory()
        {
            
            Repository.CategoryRepository catRepo = new Repository.CategoryRepository();

          
            //ViewBag.Category = catRepo.GetAllCategory();
           
            ViewBag.Laptops = catRepo.GetAllCatLaps();
            ViewBag.Smartphones = catRepo.GetAllCatPhone();
            return View();
        }
        [HttpGet]
        public IActionResult EditCategory(string id) {
            Repository.CategoryRepository catManRepo = new Repository.CategoryRepository();
            var category=catManRepo.GetCatById(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult AddCategory(string Name, string CategoryType)
        {
            

            

            // Redirect to the appropriate view or return success message
            return RedirectToAction("Display");
        }
        [HttpPost]
        public IActionResult EditCategory(string id, string Name)
        {
            CategoryRepository catRepo = new CategoryRepository();
            catRepo.EditCategory(new Models.CategoryModel { Id=id, Name = Name });

            // Redirect to the display page after saving
            return RedirectToAction("Display");
        }
    }
}
