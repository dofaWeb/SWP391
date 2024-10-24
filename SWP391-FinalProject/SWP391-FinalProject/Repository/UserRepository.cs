using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Security.Principal;

namespace SWP391_FinalProject.Repository
{
    public class UserRepository
    {
        DBContext db;
        public UserRepository()
        {
            db = new DBContext();
        }
        public UserModel GetUserProfileByUsername(string username)
        {
            using (DBContext dbContext = new DBContext())
            {
                // Truy vấn dữ liệu bằng LINQ
                var userProfile = (from a in dbContext.Accounts
                                   join u in dbContext.Users on a.Id equals u.AccountId
                                   where a.Username == username
                                   select new UserModel
                                   {
                                       Account = new AccountModel
                                       {
                                           Id = a.Id,
                                           Username = a.Username,
                                           Password = a.Password,
                                           Name = a.Username,
                                           Email = a.Email,
                                           Phone = a.Phone,
                                           Status = (a.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                                           RoleId = a.RoleId,
                                           RoleName = dbContext.RoleNames
                                                          .Where(r => r.Id == a.RoleId)
                                                          .Select(r => r.Name).FirstOrDefault(), // Lấy tên vai trò
                                       },
                                       Name = u.Name,
                                       Point = u.Point,
                                       Province = u.Province,
                                       District = u.District,
                                       Address = u.Address
                                   }).FirstOrDefault();

                return userProfile;
            }
        }

        public UserModel GetUserProfileByUserId(string userId)
        {
            using (DBContext dbContext = new DBContext())
            {
                // Truy vấn dữ liệu bằng LINQ
                var userProfile = (from a in dbContext.Accounts
                                   join u in dbContext.Users on a.Id equals u.AccountId
                                   where a.Id == userId
                                   select new UserModel
                                   {
                                       Account = new AccountModel
                                       {
                                           Id = a.Id,
                                           Username = a.Username,
                                           Password = a.Password,
                                           Name = a.Username,
                                           Email = a.Email,
                                           Phone = a.Phone,
                                           Status = (a.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                                           RoleId = a.RoleId,
                                           RoleName = dbContext.RoleNames
                                                          .Where(r => r.Id == a.RoleId)
                                                          .Select(r => r.Name).FirstOrDefault(), // Lấy tên vai trò
                                       },
                                       Name = u.Name,
                                       Point = u.Point,
                                       Province = u.Province,
                                       District = u.District,
                                       Address = u.Address
                                   }).FirstOrDefault();

                return userProfile;
            }
        }
        public string GetUserIdByUserName(string UserName)
        {
            var UserId = (from a in db.Accounts
                          where a.Username == UserName
                          select a.Id).FirstOrDefault();
            return UserId;
        }
        public void UpdateUser(UserModel User)
        {
            AccountRepository accRepo = new AccountRepository();
            AccountModel Acc = accRepo.GetUserByUsernameOrEmail(User.Account.Username);
            User.Account.Id = Acc.Id;
            User.Account.Status = Acc.Status;
            User.Account.RoleId = Acc.RoleId;
            accRepo.UpdateAccount(User.Account);
            var existingUser = db.Users.FirstOrDefault(u => u.AccountId == User.Account.Id);
            if (existingUser != null)
            {
                // Cập nhật các thuộc tính của tài khoản
                existingUser.Name = User.Name;
                existingUser.Province = User.Province;
                existingUser.District = User.District;
                existingUser.Address = User.Address;
                if (User.Point != 0)
                {
                    existingUser.Point = User.Point;
                }
                db.SaveChanges();
            }
        }

        public void UpdateUserPoint(string username, int OrderStateId , decimal? UsePoint, decimal? EarnPoint)
        {
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(username);
            if (OrderStateId == 1)
            {
                user.Point -= UsePoint.HasValue ? (int)UsePoint.Value : 0;
            }else if (OrderStateId == 2)
            {
                user.Point += EarnPoint.HasValue ? (int)EarnPoint.Value : 0;
            }else if (OrderStateId == 3)
            {
                user.Point += UsePoint.HasValue ? (int)UsePoint.Value : 0;
            }
            userRepo.UpdateUser(user);
            db.SaveChanges();
        }

        public void BanUserById(string id)
        {
            var user = db.Accounts.Where(p => p.Id == id).FirstOrDefault();
            user.IsActive = (user.IsActive == ulong.Parse("1")) ? ulong.Parse("0") : ulong.Parse("1");
            db.SaveChanges();
        }
    }
}
