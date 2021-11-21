using LLU.Android.LLU.Models;
using LLU.Models;

namespace LLU.Android.Controllers
{
    internal static class AccountController
    {
        public static UserData UserData
        {
            get
            {
                var user = User.Database.GetUserData().Result;
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.Username) || !string.IsNullOrEmpty(user.Password))
                        //Normally a connection to server would fail if you supply a bad username or password, but the goal is to make as little as possible connections to the server to preserve cycles both on server and on smartphone.
                        return user;
                }
                return null;
            }
        }
        public static bool CreateEmailUser(UserData userdata)
        {
            if (EmailUser.EmailUserData == null)
            {
                EmailUser.EmailUserData = new EmailUser(userdata.Username, userdata.Password);
            }
            else
            {
                EmailUser.EmailUserData.SetUserName(userdata.Username);
                EmailUser.EmailUserData.SetPassword(userdata.Password);
            }

            return EmailUser.EmailUserData.IsClientConnected;
        }
        public static byte Login(UserData temp)
        {
            if (temp is null) return 2;
            if (CreateEmailUser(temp))
                if (EmailUser.EmailUserData.IsClientAuthenticated)
                    return 0;
                else
                    return 2;
            return 1;
        }
        public static byte Login(string username, string password)
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