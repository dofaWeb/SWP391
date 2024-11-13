using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class ComController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Display()
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            var querry = comRepo.GetAllComments();
            return View(querry);
        }
        [HttpPost]
        public IActionResult SearchedComment(string keyword, DateTime? fromDate, DateTime? toDate)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            var querry = comRepo.SearchedComment(keyword, fromDate, toDate);
            return View(querry);
        }
        [HttpGet]
        public IActionResult DeleteCommentAdmin(string id)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.DeleteAllReply(id);
            comRepo.DeleteComment(id);

            return RedirectToAction("Display");
        }
        [HttpGet]
        public IActionResult DeleteReply(string Id)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();

            comRepo.DeleteReply(Id);

            return RedirectToAction("Display");
        }
        [HttpPost]
        public IActionResult DeleteComment(string id)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.DeleteAllReply(id);
            comRepo.DeleteComment(id);
            return Ok();
        }
        [HttpPost]
        public IActionResult UpdateComment(string id, string comment)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.UpdateComment(id, comment);
            return Ok();
        }
        [HttpPost]
        public IActionResult UpdateReply(string id, string comment)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.UpdateReply(id, comment);
            return RedirectToAction("Display");
        }
        [HttpPost]
        public IActionResult AddComment(CommentModel model, string Username, string ProductItemId, decimal Price)
        {
            ComRepository comRep = new ComRepository();
            UserRepository userRep = new UserRepository();
            model.UserId = userRep.GetUserIdByUserName(Username);
            comRep.AddComment(model);
            return RedirectToAction("ProductDetail", "Pro", new { id = model.ProductId, productItemId = ProductItemId, Price = Price });

        }
        [HttpPost]
        public IActionResult ReplytoComment(string CommentId, ReplyCommentModel model)
        {
            ComRepository comRep = new ComRepository();
            model.CommentId = CommentId;
            comRep.AddReply(model);
            return RedirectToAction("Display");
        }
    }
}
