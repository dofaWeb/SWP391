﻿using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]
        public IActionResult DeleteCommentAdmin(string id)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.DeleteComment(id);
            return RedirectToAction("Display");
        }
        [HttpPost]
        public IActionResult DeleteComment(string id)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.DeleteComment(id);
            return Ok();
        }
        [HttpPost]
        public IActionResult UpdateComment(string id, string comment)
        {
            Repository.ComRepository comRepo = new Repository.ComRepository();
            comRepo.UpdateComment(id,comment);
            return Ok();
        }
        [HttpPost]
        public IActionResult AddComment(CommentModel model,string Username)
        {
            ComRepository comRep = new ComRepository();
            UserRepository userRep = new UserRepository();
            model.UserId = userRep.GetUserIdByUserName(Username);
            comRep.AddComment(model);
            return RedirectToAction("ProductDetail", "Pro", new { id = model.ProductId });

        }
    }
}