using LLU.Models;
using MimeKit;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace LLU.Android.Controllers
{
    public class DatabaseManagement
    {
        readonly SQLiteAsyncConnection database;
        public DatabaseManagement(string DbPath)
        {
            database = new SQLiteAsyncConnection(DbPath);
            database.CreateTableAsync<DatabaseData>().Wait();
            database.CreateTableAsync<UserData>().Wait();
            database.CreateTableAsync<EmailIDs>().Wait();
            database.CreateTableAsync<EmailHistory>().Wait();
            CleanupDatabase();
        }
        private void CleanupDatabase()
        {
            //todo find a better way to get rid of empty entries;
            //Textbox check mostlikely
            var userdata = GetUserData();
            foreach (var item in userdata.Result)
            {
                if (string.IsNullOrEmpty(item.Password) || string.IsNullOrEmpty(item.Username))
                {
                    Task<int> task = database.DeleteAsync<UserData>(item.UserID);
                }
            }
        }
        public Task<List<UserData>> GetUserData()
        {
            return database.Table<UserData>().ToListAsync();
        }
        public Task<List<DatabaseData>> GetEmailData()
        {
            return database.Table<DatabaseData>().ToListAsync();
        }
        public List<string> GetPresentEmail(string userid)
        {
            var data = database.Table<EmailIDs>().ToListAsync();
            var idslist = new List<string>();
            foreach(var id in data.Result)
            {
                if (id.UserID.Equals(userid))
                {
                    idslist.Add(id.MessageID);
                }
            }
          
            return idslist;
        }
        public void AddToHistory(int amount, string userid)
        {
            database.InsertOrReplaceAsync(new EmailHistory { UserID = userid, Emails = amount});
        }
        public int GetEmailCount(string userid)
        {
            try
            {
                var x =  database.Table<EmailHistory>()
                    .Where(id => id.UserID.Equals(userid))
                    .FirstAsync()
                    .Result;
                if (x != null)
                    return x.Emails;
                return 0;
            }
            catch
            {
                return 0;
            }

        }
        public bool SaveAllEmailAsync(List<MimeMessage> data, string userID)
        {
            //Check if email is already present in DB and add missing emails.
            var db = GetPresentEmail(userID);
            var x = GetEmailCount(userID);
            if (db.Count == x && db.Count != 0)
                return true;
            try
            {
                foreach (var item in data)
                {
                    DatabaseData datatemp = new();
                    datatemp.Id = item.MessageId;
                    datatemp.UserID = userID;
                    datatemp.From = item.From.ToString();
                    datatemp.To = item.To.ToString();
                    datatemp.Subject = item.Subject;
                    datatemp.Time = item.Date;
                    if (item.HtmlBody != null)
                        datatemp.Body = item.HtmlBody;
                    else datatemp.Body = item.TextBody;
                    if (!db.Exists(id => id.Equals(item.MessageId)))
                    {
                        database.InsertAsync(datatemp);
                        database.InsertAsync(new EmailIDs { UserID = userID, MessageID = item.MessageId });
                    }
                }
                AddToHistory(db.Count, userID);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public Task<int> SaveUserAsync(UserData data) => database.InsertAsync(data, typeof(UserData));

    }
}