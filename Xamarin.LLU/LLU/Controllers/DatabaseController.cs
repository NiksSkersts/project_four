using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LLU.Controllers;
using LLU.Models;
using MimeKit;
using SQLite;

#nullable enable
namespace LLU.Android.Controllers;

public class DatabaseController : IController {
    private static DatabaseController? _dbController;
    private readonly SQLiteAsyncConnection _database;
    internal MailStorageSystem? RuntimeDatabase { get; set; }

    private DatabaseController() {
        _database = (SQLiteAsyncConnection) Connect(DataController.GetFilePath("data"));
        var result = _database.DropTableAsync<DatabaseData>().Result;
        var createTableResult = _database.CreateTableAsync<DatabaseData>().Result;
    }
    internal static DatabaseController DbController => _dbController ??= new DatabaseController();

    internal void UpdateDatabase(ObservableCollection<DatabaseData> list) {
        foreach (var message in list) {
            message.Id ??= message.UniqueId;
            _ = _database.InsertOrReplaceAsync(message).Result;
        }
        RuntimeDatabase = UpdateRuntimeDatabase();
    }
    internal MailStorageSystem UpdateRuntimeDatabase() {
        var currentData = _database.Table<DatabaseData>().ToListAsync().Result;
        
        var listOfYears = new List<Year>();
        
        foreach (var data in currentData) {
            var utctime = DateTimeOffset.FromUnixTimeSeconds(data.Time);
            var searchYear = listOfYears.Find(year => year.Value.Equals(utctime.Year));
            
            if (searchYear is null) {
                searchYear= new Year(utctime.Year, new List<Month>());
                listOfYears.Add(searchYear);
            }

            var searchMonth = searchYear.Months.Find(month=>month.Value.Equals(utctime.Month));
            if (searchMonth is null) {
                searchMonth = new Month(searchYear, utctime.Month, new List<DatabaseData>());
                searchYear.Months.Add(searchMonth);
            }
            searchMonth.Mail.Add(data);
        }
        return new MailStorageSystem(listOfYears);
    }
    
    public object Connect(object data) 
        => new SQLiteAsyncConnection((string) data);
    public object ClientAuth(UserData temp, object? db) => _database.InsertAsync(temp, typeof(UserData)).Result;

    public void Dispose() 
        => _database.CloseAsync();
    
    public int WipeDatabase() => _database.DeleteAllAsync<DatabaseData>().Result;

    public Task<UserData> GetUserData() 
        => _database!.Table<UserData>().FirstOrDefaultAsync();
    public Task<List<DatabaseData>> GetEmailData() 
        => _database!.Table<DatabaseData>().ToListAsync();

    public Task<int> DeleteMessage(string id) {
        var row = _database!.Table<DatabaseData>().Where(q => q.Id.Equals(id)).FirstOrDefaultAsync().Result;
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
}