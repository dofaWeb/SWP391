namespace SWP391_FinalProject.Models
{
    public class AccountModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public string ProvinceId { get; set; }

        public double Point {  get; set; }

        public string Address { get; set; }
    }
}
