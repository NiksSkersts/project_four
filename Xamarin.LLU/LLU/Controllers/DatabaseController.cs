using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LLU.Android.Models;
using LLU.Controllers;
using LLU.Models;
using SQLite;

#nullable enable
namespace LLU.Android.Controllers;

/// <summary>
/// Controller that creates and manages a connection with SQLITE database. Also used to keep runtime database in sync with SQLITE.
/// </summary>
public class DatabaseController : IController {
    private static DatabaseController? _dbController = new();
    private readonly SQLiteAsyncConnection _database;
    /// <summary>
    /// A runtime database, that disposes of it's data after application exit.
    /// </summary>
    private MailStorageSystem RuntimeDatabase { get; }
    /// <summary>
    /// Returns a connection with the SQLITE and if the connection is null, then creates one.
    /// </summary>
    internal static DatabaseController DbController => _dbController ??= new DatabaseController();
    /// <summary>
    /// Creates a new DatabaseController Class. This is the place where to specify SQLITE location and create it's tables.
    /// Creates the runtime database.
    /// </summary>
    private DatabaseController() {
        _database = (SQLiteAsyncConnection) Connect(DataController.GetFilePath("data"));
        _ = _database.DropTableAsync<DatabaseData>().Result;
        _ = _database.CreateTableAsync<DatabaseData>().Result;
        RuntimeDatabase = new MailStorageSystem();
        InitializeRuntimeDatabase();
    }
    /// <summary>
    /// Updates both internal database and runtime database. This is the function that tries to keep both databases in sync.
    /// </summary>
    /// <param name="list">The emails received from the server</param>
    internal void UpdateDatabase(ObservableCollection<DatabaseData> list) {

        foreach (var message in list) {
            InsertReplaceMessage(message);
            _dbController?.RuntimeDatabase.AddMail(message);
        }

        var currentData = _database.Table<DatabaseData>().ToListAsync().Result;
        var currentRuntimeDb = RuntimeDatabase.ReturnMail();

        if (list.Count != currentRuntimeDb.Count) {
            foreach (var item in currentRuntimeDb) {
                var message = list.Any(q => q.Id.Equals(item.Id));
                if (message is false) {
                    RemoveMessageFromDatabase(item);
                    RuntimeDatabase.RemoveMail(item);
                }
            }
        }
    }
    
    private void RemoveMessageFromDatabase(DatabaseData message) => _database.Table<DatabaseData>().DeleteAsync(q=>q.Id.Equals(message.Id));
    private void InsertReplaceMessage(DatabaseData message) => _database.InsertOrReplaceAsync(message);
    private List<DatabaseData> GetDataFromDatabase => _database.Table<DatabaseData>().ToListAsync().Result;
    private void InitializeRuntimeDatabase() 
        => _dbController?.RuntimeDatabase
            .AddMail(GetDataFromDatabase);

    public object Connect(object data) 
        => new SQLiteAsyncConnection((string) data);
    public object ClientAuth(UserData temp, object? db) => _database.InsertAsync(temp, typeof(UserData)).Result;

    public void Dispose() {
        _database.CloseAsync();
    }
    
    public int WipeDatabase() => _database.DeleteAllAsync<DatabaseData>().Result;

    public Task<int> DeleteMessage(string id) {
        var row = _database.Table<DatabaseData>().Where(q => q.Id.Equals(id)).FirstOrDefaultAsync().Result;
        row.DeleteFlag = true;
        return _database.InsertOrReplaceAsync(row);
    }
    private int GetCurrentMessageCount() => _database.Table<DatabaseData>().CountAsync().Result;
    
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