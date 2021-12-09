using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LLU.Controllers;
using LLU.Models;
using MimeKit;
using SQLite;

#nullable enable
namespace LLU.Android.Controllers;

public class DatabaseController : IController {
    private SQLiteAsyncConnection _database = null!;

    public DatabaseController(string dbPath) {
        var result = Connect(dbPath);
        if (result is not 0)
            throw new Exception("Database failure!");
    }

    public void Dispose() {
        _database.CloseAsync();
    }

    public bool ClientAuth(UserData temp) => _database.InsertAsync(temp, typeof(UserData)).Result is not 0;

    public byte Connect(object data) {
        byte code = 1;
        try {
            _database = new SQLiteAsyncConnection((string) data);
            _database.CreateTableAsync<DatabaseData>();
            _database.CreateTableAsync<UserData>();
            code = 0;
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }

        return code;
    }

    public int WipeDatabase() {
        var sum = _database.DeleteAllAsync<DatabaseData>().Result;
        sum += _database.DeleteAllAsync<UserData>().Result;
        return sum;
    }

    public Task<UserData> GetUserData() => _database.Table<UserData>().FirstOrDefaultAsync();

    public Task<int> DropUserTable() => _database.DeleteAllAsync<UserData>();

    public Task<List<DatabaseData>> GetEmailData() => _database.Table<DatabaseData>().ToListAsync();

    public List<string> GetPresentEmail(string username) {
        var data = _database.Table<DatabaseData>().ToListAsync();
        return (from id in data.Result where id.Username.Equals(username) select id.Id).ToList();
    }

    public Task<int> DeleteMessage(string id) {
        var row = _database.Table<DatabaseData>().Where(q => q.Id.Equals(id)).FirstOrDefaultAsync().Result;
        row.DeleteFlag = true;
        return _database.InsertOrReplaceAsync(row);
    }

    private int GetCurrentMessageCount() => _database.Table<DatabaseData>().CountAsync().Result;

    private string GetLatestMessageId() => _database.Table<DatabaseData>().ElementAtAsync(0).Id.ToString();

    /// <summary>
    ///     Basic check for change. Compares the amount of messages in database and server.
    ///     This is not as reliable as the extended check, but it should be quicker to check.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private bool CheckForChanges(int count) {
        var current = GetCurrentMessageCount();
        return current > count || current < count;
    }

    /// <summary>
    ///     Extenstion of the basic check. This is for the situations where user has done modifications to the mailbox, yet the
    ///     count remains the same.
    /// </summary>
    /// <param name="latestMessageId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool CheckForChanges(string latestMessageId, int count) {
        var isChanged = CheckForChanges(count);
        if (isChanged) return false;
        return !latestMessageId.Equals(GetLatestMessageId());
    }

    /// <summary>
    ///     Applies the changes from the server to the app database.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public int ApplyChanges(ObservableCollection<MimeMessage> data, string username) {
        var count = 0;
        if (!CheckForChanges(GetLatestMessageId(), GetCurrentMessageCount()))
            return count;
        try {
            foreach (var item in data) {
                DatabaseData datatemp = new();
                datatemp.Id = item.MessageId;
                datatemp.Username = username;
                datatemp.From = item.From.ToString();
                datatemp.To = item.To.ToString();
                datatemp.Subject = item.Subject;
                datatemp.Time = item.Date;
                if (item.HtmlBody != null) {
                    datatemp.Body = item.HtmlBody;
                    datatemp.IsHtmlBody = true;
                }
                else {
                    datatemp.Body = item.TextBody;
                }

                if (_database.Table<DatabaseData>().CountAsync(q => q.Id.Equals(item.MessageId)).Result != 0)
                    count += _database.InsertAsync(datatemp).Result;
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }

        return count;
    }

    public bool ClientAuth(string username, string password) {
        var data = new UserData {
            Username = username,
            Password = password
        };
        return ClientAuth(data);
    }
}