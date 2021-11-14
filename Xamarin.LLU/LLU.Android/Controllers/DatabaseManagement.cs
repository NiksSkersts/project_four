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
        }
        public int WipeDatabase()
        {
            var sum = database.DeleteAllAsync<DatabaseData>().Result;
            sum += database.DeleteAllAsync<UserData>().Result;
            return sum;
        }
        public Task<UserData> GetUserData() => database.Table<UserData>().FirstOrDefaultAsync();
        public Task<int> DropUserTable() => database.DeleteAllAsync<UserData>();
        public Task<List<DatabaseData>> GetEmailData() => database.Table<DatabaseData>().ToListAsync();
        public List<string> GetPresentEmail(string userid)
        {
            var data = database.Table<DatabaseData>().ToListAsync();
            var idslist = new List<string>();
            foreach (var id in data.Result)
            {
                if (id.UserID.Equals(userid))
                {
                    idslist.Add(id.Id);
                }
            }

            return idslist;
        }
        public Task<int> SaveUserAsync(UserData data) => database.InsertAsync(data, typeof(UserData));
        public Task<int> DeleteMessage(string id)
        {
            var row = database.Table<DatabaseData>().Where(q => q.Id.Equals(id)).FirstOrDefaultAsync().Result;
            row.DeleteFlag = true;
            return database.InsertOrReplaceAsync(row);
        }
        public int GetCurrentMessageCount() => database.Table<DatabaseData>().CountAsync().Result;
        public bool CheckForChanges(string userID)
        {
            var dataInDb = GetPresentEmail(userID);
            var CurrentCount = GetCurrentMessageCount();
            if (CurrentCount > dataInDb.Count || CurrentCount < dataInDb.Count)
                return true;
            return false;
        }
        public int ApplyChanges(List<MimeMessage> data, string userID)
        {
            var count = 0;
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
                    {
                        datatemp.Body = item.HtmlBody;
                        datatemp.IsHtmlBody = true;
                    }
                    else datatemp.Body = item.TextBody;
                    if (database.Table<DatabaseData>().CountAsync(q => q.Id.Equals(item.MessageId)).Result != 0)
                        count += database.InsertAsync(datatemp).Result;
                }
            }
            catch
            {
                return count;
            }
            return count;
        }

    }
}