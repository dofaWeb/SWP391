namespace SWP391_FinalProject.Models
{
    public class UserModel
    {
        public AccountModel Account { get; set; }

        public string Name { get; set; }

        public int Point { get; set; }

        public string AddressDefault { get; set; }

        public List<AddressModel> Address { get; set; }

    }
}
