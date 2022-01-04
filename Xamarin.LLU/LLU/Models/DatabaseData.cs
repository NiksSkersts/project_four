using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace LLU.Models;

/// <summary>
/// Custom class that models the internal database structure.
/// </summary>
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
    public bool NewFlag { get; set; }
    public bool PriorityFlag { get; set; }
}

/// <summary>
/// Defines the data that's associated with the user.
/// </summary>
public class UserData {
    public string Username { get; set; }
    public string Password { get; set; }
}

/// <summary>
/// Runtime database. Stores messages in a tree that has been split into years and further on - months.
/// Defines custom functions to deal with adding, removing or outputting to and from the database. 
/// </summary>
internal class MailStorageSystem {
    public List<Year> Years;
    public MailStorageSystem() => Years = new List<Year>();
    public MailStorageSystem(List<Year> years) => Years = years;

    public void AddMail(DatabaseData mail) {
        var utctime = DateTimeOffset.FromUnixTimeSeconds(mail.Time);
        var searchYear = Years.Find(year => year.Value.Equals(utctime.Year));
        if (searchYear is null) {
            searchYear= new Year(utctime.Year, new List<Month>());
            Years.Add(searchYear);
        }
        var searchMonth = searchYear.Months.Find(month=>month.Value.Equals(utctime.Month));
        if (searchMonth is null) {
            searchMonth = new Month(searchYear, utctime.Month, new List<DatabaseData>());
            searchYear.Months.Add(searchMonth);
        }

        if (searchMonth.Mail.Exists(q=>q.Id.Equals(mail.Id))) {
            return;
        }
        searchMonth.Mail.Add(mail);
    }

    public void AddMail(List<DatabaseData> mail) {
        foreach (var item in mail) {
            AddMail(item);
        }
    }
    public void RemoveMail(DatabaseData mail) {
        var utctime = DateTimeOffset.FromUnixTimeSeconds(mail.Time);
        var month = SearchMonth(utctime.Month, SearchYear(utctime.Year));
        if (month is not null) {
            for (int i = 0; i < month.Mail.Count; i++) {
                if (month.Mail[i].Id.Equals(mail.Id)) {
                    month.Mail.RemoveAt(i);
                }
            }
        }
    }
    private Year? SearchYear(int year) => Years.FirstOrDefault(y => year == y.Value);
    private Month? SearchMonth(int month, Year year) => year.Months.FirstOrDefault(m => m.Value.Equals(month));
    public List<DatabaseData>? ReturnMail(int year, int month) {
        var findYear = SearchYear(year);
        if (findYear is not null) {
            var findMonth = SearchMonth(month, findYear);
            if (findMonth is not null)
                return findMonth.Mail;
        }
        return null;
    }

    public List<DatabaseData> ReturnMail() {
        var data = new List<DatabaseData>();
        foreach (var year in Years) {
            foreach (var month in year.Months) {
                data.AddRange(month.Mail);
            }
        }

        data = data.OrderBy(q => q.Time).ToList();
        return data;
    }

    public DatabaseData? SearchMail(DatabaseData mail) {
        var utctime = DateTimeOffset.FromUnixTimeSeconds(mail.Time);
        DatabaseData? data = null;
        var searchYear = 
            SearchYear(utctime.Year);
        if (searchYear is not null) {
            var searchMonth = 
                SearchMonth(utctime.Month,searchYear);
            if (searchMonth is not null) {
                data = 
                    searchMonth.Mail.Find(q=>q.Equals(mail));
            }
        }

        return data;
    }
    public void SortDatabase() {
        var newList = Years.OrderBy(predicate=>predicate.Value).ToList();
        Years = newList;
    }
}

internal class Year {
    public readonly int Value;
    public List<Month> Months;
    public Year(int value,List<Month> months) {
        Value = value;
        Months = months;
    }
}

internal class Month {
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