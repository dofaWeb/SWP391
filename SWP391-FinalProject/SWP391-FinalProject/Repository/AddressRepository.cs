using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Repository
{
    public class AddressRepository
    {
        private readonly DBContext db;

        public AddressRepository(DBContext context)
        {
            db = context;
        }
        public string GetNewId()
        {
            string lastId = db.Addresses
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "Adr00001";
            }
            // Tách phần chữ (A) và phần số (0000001)
            string prefix = lastId.Substring(0, 3); // Lấy ký tự đầu tiên
            int number = int.Parse(lastId.Substring(3)); // Lấy phần số và chuyển thành số nguyên

            // Tăng số lên 1
            int newNumber = number + 1;

            // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
            string newId = $"{prefix}{newNumber:D5}";

            return newId;
        }
    }
}
