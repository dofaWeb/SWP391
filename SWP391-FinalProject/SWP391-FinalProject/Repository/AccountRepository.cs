﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SWP391_FinalProject.Repository
{
    public class AccountRepository
    {
        private readonly DBContext db;

        public AccountRepository()
        {
            db = new DBContext();
        }
        public string GetNewId()
        {
            string lastId = db.Accounts
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "A0000001";
            }
            // Tách phần chữ (A) và phần số (0000001)
            string prefix = lastId.Substring(0, 1); // Lấy ký tự đầu tiên
            int number = int.Parse(lastId.Substring(1)); // Lấy phần số và chuyển thành số nguyên

            // Tăng số lên 1
            int newNumber = number + 1;

            // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }

        public List<Models.AccountModel> GetAllAccount() {

            var user = from acc in db.Accounts
                       select new Models.AccountModel
                       {
                           Id = acc.Id,
                           Username = acc.Username,
  
                           Email = acc.Email,
                           Phone = acc.Phone,
      
                           Status = (acc.IsActive == ulong.Parse("1"))? "Active" : "Inactive",
                           RoleName = acc.Role.Name

                       };
            var result = user.ToList();
            return result;
        }
        
        public Models.AccountModel GetAccountById(string id)
        {
            var user = from acc in db.Accounts
                       join u in db.Users on acc.Id equals u.AccountId
                       where acc.Id == id
                       select new Models.AccountModel
                       {
                           Id = acc.Id,
                           Username = acc.Username,
                           Name = u.Name,
                           Email = acc.Email,
                           Phone = acc.Phone,
                           Point = u.Point
                       };
            var result = user.FirstOrDefault();
            return result;
        }

        

        public bool CheckEmail(string email)
        {
            var check = db.Accounts.Where(p => p.Email == email).FirstOrDefault();
            if (check != null)
            {
                return false;
            }
            return true;
        }

        public void AddStaffAccount(AccountModel model)
        {
            string id = GetNewId();
            string md5Password = MySetting.GetMd5Hash(model.Password);
            var newAccount = new Entities.Account()
            {
                Id = id,
                Username = model.Email,
                Password = md5Password,
                Email = model.Email,
                Phone = "",
                IsActive = ulong.Parse("1"),
                RoleId = "Role0002",
            };

            var newStaff = new Entities.Staff()
            {
                AccountId = id,
                Name = model.Email,
                Salary = 5000000
            };

            db.Accounts.Add(newAccount);
            db.Staff.Add(newStaff);
            db.SaveChanges();
        }

        public void AddAccount(Models.AccountModel model)
        {
            string id = GetNewId();
            string md5Password = MySetting.GetMd5Hash(model.Password);
            var newAccount = new SWP391_FinalProject.Entities.Account()
            {
                Id = id,
                Username = model.Username,
                Password = md5Password,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = ulong.Parse("1"),
                RoleId = "Role0003",
            };

            var newUser = new SWP391_FinalProject.Entities.User()
            {
                AccountId = newAccount.Id,
                Name = model.Name,
                Point = 0,
                Province = model.Province,
                District = model.District,
                Address = model.Address,
            };
            

            db.Accounts.Add(newAccount);
            db.Users.Add(newUser);

            db.SaveChanges();
        }

        public bool Login(string username, string password)
        {
            string mdPassword = MySetting.GetMd5Hash(password);
            var check = db.Accounts.Where(p => p.Username == username && p.Password == mdPassword).ToList();
            if (check.Any())
            {
                return true;
            }
            return false;
        }

        public string GetRoleId(string username)
        {
            var roleId = db.Accounts.Where(p => p.Username == username).FirstOrDefault();
            return roleId != null ? roleId.RoleId : string.Empty;
        }

        public Models.AccountModel GetUserByUsernameOrEmail(string key)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id
                           join u in db.Users on account.Id equals u.AccountId
                           where account.Username == key || account.Email == key
                           select new Models.AccountModel
                           {
                               Id = account.Id ??"",
                               Username = account.Username ?? "",
                               Password = account.Password ?? "",
                               Email = account.Email ?? "",
                               Phone = account.Phone ?? "",
                               Name = u.Name ?? "",
                               RoleId = account.RoleId ?? "",
                               Status = (account.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                               RoleName = role.Name ?? "" // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }
        public Models.AccountModel GetStaffByUsernameOrEmail(string key)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id
                          
                           where account.Username == key || account.Email == key
                           select new Models.AccountModel
                           {
                               Id = account.Id,
                               Username = account.Username,
                               Password = account.Password ,
                               Email = account.Email,
                               Phone = "",
                               
                               RoleId = account.RoleId,
                               Status = (account.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                               RoleName = role.Name  // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }

        public Models.AccountModel GetUserByUsernameAndEmail(string username, string email)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id
                           join u in db.Users on account.Id equals u.AccountId
                           where account.Username == username && account.Email == email
                           select new Models.AccountModel
                           {
                               Id = account.Id,
                               Username = account.Username,
                               Password = account.Password,
                               Email = account.Email,
                               Phone = account.Phone,
                               Name = u.Name,
                               RoleId = account.RoleId,
                               Status = account.IsActive.ToString(),
                               RoleName = role.Name // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }

        public void ResetPassword(AccountModel account)
        {
            var existingAccount = db.Accounts.FirstOrDefault(a => a.Id == account.Id);
            if (existingAccount != null)
            {
                string mdPassword = MySetting.GetMd5Hash(account.Password);
                existingAccount.Password = mdPassword;
                db.SaveChanges();
            }
        }

        public void UpdateAccount(Models.AccountModel account)
        {

            // Tìm bản ghi theo Id từ cơ sở dữ liệu
            var existingAccount = db.Accounts.FirstOrDefault(a => a.Id == account.Id);

            if (existingAccount != null)
            {
                // Cập nhật các thuộc tính của tài khoản
                if (account.Email != null)
                    existingAccount.Email = account.Email;
                if (account.Phone != null)
                    existingAccount.Phone = account.Phone;
                if (account.Status != null)
                {
                    if (account.Status == "Active")
                    {
                        existingAccount.IsActive = ulong.Parse("1");
                    }
                    else
                    {
                        existingAccount.IsActive = ulong.Parse("0");
                    }
                }
                if (account.RoleId != null)
                    existingAccount.RoleId = account.RoleId;
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }

        }

    }
}
