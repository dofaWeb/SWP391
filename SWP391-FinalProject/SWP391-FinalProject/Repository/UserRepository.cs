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
                                   join ua in dbContext.UserAddresses on u.AccountId equals ua.UserId
                                   join ad in dbContext.Addresses on ua.AddressId equals ad.Id
                                   join p in dbContext.Provinces on ad.ProvinceId equals p.Id
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
                                           ProvinceId = ad.ProvinceId,
                                           Address = ad.Address1
                                       },
                                       Name = u.Name,
                                       Point = u.Point,
                                       AddressDefault = ad.Address1,
                                       Address = dbContext.UserAddresses
                                                          .Where(ua => ua.UserId == u.AccountId)
                                                          .Join(dbContext.Addresses,
                                                                ua => ua.AddressId,
                                                                ad => ad.Id,
                                                                (ua, ad) => new AddressModel
                                                                {
                                                                    Id = ad.Id,
                                                                    ProvinceId = ad.ProvinceId,
                                                                    Address = ad.Address1,
                                                                    Province = ad.Province.Name
                                                                }).ToList()
                                   }).FirstOrDefault();

                return userProfile;
            }
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
                
                db.SaveChanges();
            }
        }


    }
}
