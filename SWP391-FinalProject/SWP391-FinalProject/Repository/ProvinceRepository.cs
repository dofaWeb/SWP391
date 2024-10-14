﻿using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Repository
{
    public class ProvinceRepository
    {
        private readonly DBContext db;
        public ProvinceRepository()
        {
            this.db = new DBContext();
        }

        public List<Models.ProvinceModel> GetAllProvince()
        {
            var result = db.Provinces.Select(p => new Models.ProvinceModel { Id = p.Id, Name = p.Name, }).ToList();
            return result;
        }
    }
}
