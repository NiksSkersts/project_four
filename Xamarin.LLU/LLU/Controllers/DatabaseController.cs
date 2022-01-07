using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Kotlin;
using LLU.Android.Models;
using LLU.Controllers;
using LLU.Models;
using SQLite;

#nullable enable
namespace LLU.Android.Controllers;

public interface IDatabase {
    /// <summary>
    ///     Updates both internal database and runtime database. This is the function that tries to keep both databases in
    ///     sync.
    /// </summary>
    /// <param name="list">The emails received from the server</param>
    public void UpdateDatabase(ObservableCollection<DatabaseData> list);

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    public void UpdateDatabase(DatabaseData message);

    /// <summary>
    ///     Removes mail by using MessageID.
    ///     Mail that is in database does not get deleted per se, but is replaced by the same row with deleteFlag set to true.
    ///     Deleted mail isn't exactly deleted for ever, but is moved to "Deleted" folder.
    ///     Keeping such data in RuntimeDatabase would be detrimental to processing speed,
    ///     but it can still be retrieved just in case from SQLITE db.
    /// </summary>
    /// <param name="id">MessageID or if MessageID is null, then UniqueID.</param>
    /// <returns></returns>
    public bool RemoveMessage(string id);

    /// <summary>
    ///     Deletes mail by using mail object.
    ///     Removing mail by using object is much faster, as it also provides time to work with.
    ///     It cuts down on the amount of mail CPU has to sift down.
    ///     Removing from SQLITE is unchanged, as the messageID is also the primary key for the row object.
    /// </summary>
    /// <param name="mail">DatabaseData object</param>
    /// <returns></returns>
    public bool RemoveMessage(DatabaseData mail);

    /// <summary>
    ///     Wipes Runtime and SQLITE database.
    /// </summary>
    /// <returns></returns>
    public bool WipeDatabase();
}

public class RuntimeController : DatabaseController, IDatabase, IDisposable {
    private RuntimeController() {
        RuntimeDatabase.AddMail(GetDataFromDatabase());
    }

    public static RuntimeController Instance { get; } = new();

    /// <summary>
    ///     A runtime database, that disposes of it's data after application destroy().
    /// </summary>
    private MailStorageSystem RuntimeDatabase { get; } = new();

    public void UpdateDatabase(ObservableCollection<DatabaseData> list) {
        foreach (var message in list) UpdateDatabase(message);
    }

    public void UpdateDatabase(DatabaseData message) {
        RuntimeDatabase.AddMail(message);
        InsertReplaceMessage(message);
    }

    public bool RemoveMessage(string id) {
        var result = RuntimeDatabase.RemoveMail(id);
        var deleteMessage = DeleteMessage(id);
        return result && deleteMessage.Result is not 0;
    }

    public bool RemoveMessage(DatabaseData mail) {
        var result = RuntimeDatabase.RemoveMail(mail);
        var deleteMessage = DeleteMessage(mail.Id);
        return result && deleteMessage.Result is not 0;
    }

    public new bool WipeDatabase() =>
        RuntimeDatabase.RemoveMail(true) && base.WipeDatabase();

    public new void Dispose() {
        base.Dispose();
    }

    public ObservableCollection<DatabaseData> GetMessages() {
        var data = new ObservableCollection<DatabaseData>();
        foreach (var item in RuntimeDatabase.ReturnMail()) data.Add(item);
        return data;
    }
    [Obsolete]
    public void SyncDatabase() {
        var currentData = Database.Table<DatabaseData>().ToListAsync().Result;
        var currentRuntimeDb = RuntimeDatabase.ReturnMail();

        if (currentData.Count != currentRuntimeDb.Count)
            foreach (var item in currentRuntimeDb) {
                var message = currentData.Any(q => q.Id.Equals(item.Id));
                if (message is false) {
                    RemoveMessageFromDatabase(item);
                    RuntimeDatabase.RemoveMail(item);
                }
            }
    }
}

/// <summary>
///     Controller that creates and manages a connection with SQLITE database. Also used to keep runtime database in sync
///     with SQLITE.
/// </summary>
public class DatabaseController : IController {
    protected readonly SQLiteAsyncConnection Database;

    /// <summary>
    ///     Creates a new DatabaseController Class. This is the place where to specify SQLITE location and create it's tables.
    ///     Creates the runtime database.
    /// </summary>
    protected DatabaseController() {
        Database = (SQLiteAsyncConnection) Connect(DataController.GetFilePath("data"));
        _ = Database.DropTableAsync<DatabaseData>().Result;
        _ = Database.CreateTableAsync<DatabaseData>().Result;
    }

    protected List<DatabaseData> GetDataFromDatabase() {
        var tableData = Database.Table<DatabaseData>().ToListAsync().Result;
        return tableData.Where(q => q.DeleteFlag == false).ToList();
    }

    public object Connect(object data)
        => new SQLiteAsyncConnection((string) data);

    public object ClientAuth(UserData temp, object? db) => Database.InsertAsync(temp, typeof(UserData)).Result;

    public void Dispose() {
        Database.CloseAsync();
    }

    protected void RemoveMessageFromDatabase(DatabaseData message) =>
        Database.Table<DatabaseData>().DeleteAsync(q => q.Id.Equals(message.Id));

    protected void InsertReplaceMessage(DatabaseData message) => Database.InsertOrReplaceAsync(message);

    protected bool WipeDatabase() => Database.DeleteAllAsync<DatabaseData>().Result is not 0;

    protected Task<int> DeleteMessage(string id) {
        var row = Database.Table<DatabaseData>().Where(q => q.Id.Equals(id)).FirstOrDefaultAsync().Result;
        if (row is not null) {
            row.DeleteFlag = true;
        }
        return Database.InsertOrReplaceAsync(row);
    }
}