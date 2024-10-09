using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Repository
{
    public class Province
    {
        private readonly DBContext db;
        public Province(DBContext context)
        {
            this.db = context;
        }

        public List<Models.ProvinceModel> GetAllProvince()
        {
            var result = db.Provinces.Select(p => new Models.ProvinceModel { Id = p.Id, Name = p.Name, }).ToList();
            return result;
        }
    }
}
