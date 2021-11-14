using LLU.Android.LLU.Models;
using LLU.Models;

namespace LLU.Android.Controllers
{
    internal static class AccountManager
    {
        public static UserData UserData
        {
            get
            {
                var user = User.Database.GetUserData().Result;
                if(user != null)
                {
                    if (!string.IsNullOrEmpty(user.Username) || !string.IsNullOrEmpty(user.Password))
                    //Normally a connection to server would fail if you supply a bad username or password, but the goal is to make as little as possible connections to the server to preserve cycles both on server and on smartphone.
                        return user;
                }
                return null;
            }
        }
        public static bool Login(UserData userdata)
        {
            EmailUser.EmailUserData = new EmailUser(userdata.Username, userdata.Password);
            if (!EmailUser.EmailUserData.GetClient().IsConnected)
                EmailUser.EmailUserData = null;
            // EmailUserData being null indicates that something is wrong with login. e.g. changed password or no connection.
            if (EmailUser.EmailUserData != null)
                return true;
            return false;
        }
        public static bool Login(string username, string password)
        {
            UserData temp = new()
            {
                Username = username,
                Password = password
            };
            return Login(temp);
        }
    }
}