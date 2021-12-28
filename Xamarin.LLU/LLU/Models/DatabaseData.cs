using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Text;
using MimeKit;
using MimeKit.Text;
using SQLite;

namespace LLU.Models;

public class DatabaseData {
    [PrimaryKey] public string Id { get; set; }
    public string UniqueId { get; set; }
    public string Folder { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public long Time { get; set; }
    public string Body { get; set; }
    public bool IsHtmlBody { get; set; }
    public bool DeleteFlag { get; set; }
}

public class UserData {
    public string Username { get; set; }
    public string Password { get; set; }
}

internal class MailStorageSystem {
    public List<Year> Years;

    public MailStorageSystem(List<Year> years) => Years = years;
    public Year? SearchYear(int year) => Years.FirstOrDefault(y => year == y.Value);
    public Month? SearchMonth(int month, Year year) => year.Months.FirstOrDefault(m => m.Value.Equals(month));
    public List<DatabaseData>? ReturnMail(int year, int month) {
        var findYear = SearchYear(year);
        if (findYear is not null) {
            var findMonth = SearchMonth(month, findYear);
            if (findMonth is not null)
                return findMonth.Mail;
        }
        return null;
    }
    public void SortDatabase() {
        var newList = Years.OrderBy(predicate=>predicate.Value).ToList();
        Years = newList;
    }
}

class Year {
    public readonly int Value;
    public List<Month> Months;
    public Year(int value,List<Month> months) {
        Value = value;
        Months = months;
    }
}

class Month {
    public Year Year;
    public int Value;
    public List<DatabaseData> Mail;
    public int MailCount;

    public Month(Year year,int month,List<DatabaseData> mail) {
        Value = month;
        Year = year;
        Mail = mail;
        MailCount = mail.Count;
    }
}