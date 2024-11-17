using Microsoft.AspNetCore.Mvc.Filters;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Filters
{
    public class UserAuthorizationFilter 
    {
        public static bool CheckUser(string UserId)
        {
            UserRepository userRepository = new UserRepository();
            if (!userRepository.CheckBan(UserId))
            {
                return true;
            }
            return false;
        }
    }
}
