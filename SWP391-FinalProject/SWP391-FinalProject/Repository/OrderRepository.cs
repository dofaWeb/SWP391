using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class OrderRepository
    {
        private readonly DBContext db;
        public OrderRepository()
        {
            db = new DBContext();
        }
        




    }
}
