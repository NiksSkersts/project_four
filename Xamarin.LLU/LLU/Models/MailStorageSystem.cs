using System;
using System.Collections.Generic;
using System.Linq;
using LLU.Models;

namespace LLU.Android.Models;
/// <summary>
/// Provides extensions to the base class. 
/// </summary>
internal class MailStorageSystem : MailStorageSystemBase, IMailStorageSystem {
    public void AddMail(List<DatabaseData> mail) {
        foreach (var item in mail) base.AddMail(item);
    }
}
public interface IMailStorageSystem {
    /// <summary>
    ///     Wrapper around single mail function to add multiple mails.
    /// </summary>
    /// <param name="mail"></param>
    public void AddMail(List<DatabaseData> mail);
    /// <summary>
    ///     Add a single mail to the runtime database.
    ///     This parses DatabaseData.Time field and sorts mail by their years and months.
    /// </summary>
    /// <param name="mail"></param>
    public void AddMail(DatabaseData mail);

    /// <summary>
    ///     Remove the specified mail from runtime database.
    /// </summary>
    /// <param name="mail"></param>
    public bool RemoveMail(DatabaseData mail);

    public List<DatabaseData>? ReturnMail(int year, int month, int day);
    /// <summary>
    ///     Return all mail from the specified year and month
    /// </summary>
    /// <param name="year">integer</param>
    /// <param name="month">integer</param>
    /// <returns></returns>
    public List<DatabaseData>? ReturnMail(int year, int month);

    public List<DatabaseData>? ReturnMail(int year);
    /// <summary>
    ///     Returns all available mail.
    /// </summary>
    /// <returns></returns>
    public List<DatabaseData> ReturnMail();
    /// <summary>
    ///     Find the specified mail in all database.
    /// </summary>
    /// <param name="mail">DatabaseData object.</param>
    /// <returns></returns>
    public DatabaseData? SearchMail(DatabaseData mail);
}
/// <summary>
///     Runtime database. Stores messages in a tree that has been split into years and further on - months.
///     Defines custom functions to deal with adding, removing or outputting to and from the database.
/// </summary>
internal abstract class MailStorageSystemBase {
    private static List<Year> _years = new();
    /// <summary>
    ///     Constructor that creates an empty class.
    /// </summary>
    public void AddMail(DatabaseData mail) {
        var utctime = DateTimeOffset.FromUnixTimeSeconds(mail.Time);
        var searchYear = _years.Find(year => year.Value.Equals(utctime.Year));
        if (searchYear is null) {
            searchYear = new Year(utctime.Year);
            _years.Add(searchYear);
        }
        searchYear.AddMail(mail);
    }
    public bool RemoveMail(DatabaseData mail) {
        var searchYear = SearchYear(DateTimeOffset.FromUnixTimeSeconds(mail.Time).Year);
        return searchYear is not null && searchYear.RemoveMail(mail);
    }

    public List<DatabaseData>? ReturnMail(int year, int month, int day) {
        var findYear = SearchYear(year);
        if (findYear is null) return null;
        var findMonth = SearchMonth(month, findYear);
        if (findMonth is null) return null;
        var findDay = SearchDay(day, findMonth);
        return findDay?.ReturnMail().Values.ToList();
    }
    public List<DatabaseData>? ReturnMail(int year, int month) {
        var findYear = SearchYear(year);
        if (findYear is null) return null;
        var findMonth = SearchMonth(month, findYear);
        return findMonth?.ReturnMail();
    }
    public List<DatabaseData>? ReturnMail(int year) => SearchYear(year)?.ReturnMail();
    public List<DatabaseData> ReturnMail() {
        var data = new List<DatabaseData>();
        foreach (var year in _years) data.AddRange(year.ReturnMail());
        data = data.OrderBy(q => q.Time).ToList();
        return data;
    }
    private static Day? SearchDay(int day, Month month) => month.Days[day];
    /// <summary>
    ///     Search the specified month in the Year class.
    /// </summary>
    /// <param name="month">integer that represents the specified month.</param>
    /// <param name="year">Year class, that has a not null month list.</param>
    /// <returns></returns>
    private static Month? SearchMonth(int month, Year year) => year.Months[month];
    /// <summary>
    ///     Search the specified year in runtime database.
    /// </summary>
    /// <param name="year"> integer that represents the year to be searched for.</param>
    /// <returns></returns>
    private Year? SearchYear(int year) => _years.FirstOrDefault(y => year == y.Value);
    
    public DatabaseData? SearchMail(DatabaseData mail) {
        var utctime = DateTimeOffset.FromUnixTimeSeconds(mail.Time);
        DatabaseData? data = null;
        var searchYear = SearchYear(utctime.Year);
        if (searchYear is null) return data;
        data = searchYear.FindMail(mail);
        return data;
    }

    /// <summary>
    ///     Year object that stores integer value representing a year, and a list of months.
    /// </summary>
    internal class Year {
        public readonly Month[] Months = new Month[12];
        public readonly int Value;
        public Year(int value) => Value = value;

        public List<DatabaseData> ReturnMail() {
            var data = new List<DatabaseData>();
            for (var i = 0; i < Months.Length; i++) {
                if (Months[i] is null) continue;
                data.AddRange(Months[i].ReturnMail());
            }
            return data;
            
        }

        public void AddMail(DatabaseData mail) {
            var month = DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month;
            var searchMonth = Months[month-1];
            if (searchMonth is null) {
                Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month-1] = new Month(this,month);
            }
            Months[month-1].AddMail(mail);
        }
        public bool RemoveMail(DatabaseData mail) {
            var searchMonth = Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month-1];
            return searchMonth is not null && searchMonth.RemoveMail(mail);
        }

        public DatabaseData? FindMail(DatabaseData mail) => 
            Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month-1].FindMail(mail);
    }

    internal class Month {
        public readonly Day[] Days;
        private readonly Year Year;
        public int Value;

        public Month(Year year, int month) {
            Value = month;
            Year = year;
            Days = new Day[DateTime.DaysInMonth(Year.Value, Value)];
        }

        public List<DatabaseData> ReturnMail() {
            var data = new List<DatabaseData>();
            for (var j = 0; j < Days.Length; j++) {
                if (Days[j] is null) continue;
                data.AddRange(Days[j].ReturnMail().Values);
            }
            return data;
        }

        public void AddMail(DatabaseData mail) {
            var day = DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day;
            var searchDay = Days[day-1];
            if (searchDay is null) {
                Days[day-1] = new Day(day,this,new SortedList<long, DatabaseData>());
            }
            Days[day-1].AddMail(mail);
        }

        public bool RemoveMail(DatabaseData mail) {
            var day = DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day-1;
            var searchDay = Days[day];
            return searchDay is not null && Days[day].RemoveMail(mail);
        }

        public DatabaseData? FindMail(DatabaseData mail) => 
            Days[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day-1].FindMail(mail);
    }

    internal class Day {
        private Month Month;
        public int Value;
        public SortedList<long,DatabaseData> Mail;
        public int MailCount;
        
        public Day(int day, Month month, SortedList<long,DatabaseData> mail) {
            Month = month;
            Value = day;
            Mail = mail;
            MailCount = mail.Count;
        }

        public SortedList<long,DatabaseData> ReturnMail() => Mail;

        public void AddMail(DatabaseData mail) {
            var containsValue = false;
            foreach (var item in Mail) {
                if (item.Value.Id.Equals(mail.Id)) {
                    containsValue = true;
                }
            }
            if (containsValue)
                return;
            var containsKey = false;
            foreach (var item in Mail) {
                if (item.Key.Equals(mail.Time)) {
                    containsKey = true;
                }
            }

            if (containsKey) {
                mail.Time += 1;
            }
            Mail.Add(mail.Time,mail);
        }

        public bool RemoveMail(DatabaseData mail) {
            var data = Mail[mail.Time];
            var res = false;
            if (data.Id.Equals(mail.Id))
            {
                res = Mail.Remove(mail.Time);
            }
            return res;
        }

        public DatabaseData? FindMail(DatabaseData mail) {
            var data = Mail[mail.Time];
            if (data.Id.Equals(mail.Id))
            {
                return data;
            }
            return null;
        }
    }
}