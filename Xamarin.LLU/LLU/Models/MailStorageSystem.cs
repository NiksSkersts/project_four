using System;
using System.Collections.Generic;
using System.Linq;
using LLU.Models;

namespace LLU.Android.Models;

/// <summary>
///     Provides extensions to the base class.
/// </summary>
internal class MailStorageSystem : MailStorageSystemBase, IMailStorageSystem {
    public void AddMail(List<DatabaseData> mail) {
        foreach (var item in mail) base.AddMail(item);
    }

    public bool RemoveMail(string id) {
        var result = ReturnMail(id);
        return result is not null && RemoveMail(result);
    }
}

public interface IMailStorageSystem {
    /// <summary>
    ///     Wrapper around single mail function to add multiple mails.
    /// </summary>
    /// <param name="mail">DatabaseData object</param>
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

    /// <summary>
    ///     Clears the database.
    /// </summary>
    /// <param name="all"></param>
    /// <returns></returns>
    public bool RemoveMail(bool all);

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
    private static readonly List<Year> _years = new();

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

    public bool RemoveMail(bool all) {
        if (all) _years.Clear();

        return _years.Any();
    }

    public List<DatabaseData>? ReturnMail(int year, int month, int day) {
        var findYear = SearchYear(year);
        if (findYear is null) return null;
        var findMonth = SearchMonth(month, findYear);
        if (findMonth is null) return null;
        var findDay = SearchDay(day, findMonth);
        return findDay?.ReturnMail();
    }

    public List<DatabaseData>? ReturnMail(int year, int month) {
        var findYear = SearchYear(year);
        if (findYear is null) return null;
        var findMonth = SearchMonth(month, findYear);
        return findMonth?.ReturnMail();
    }

    public List<DatabaseData>? ReturnMail(int year) => SearchYear(year)?.ReturnMail();

    public DatabaseData? ReturnMail(string id) {
        DatabaseData? data = null;
        foreach (var mail in ReturnMail())
            if (mail.Id.Equals(id) || mail.UniqueId.Equals(id))
                data = mail;
        return data;
    }

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

    private interface IItem {
        /// <summary>
        ///     Defines the value of the Item object. E.g. year, month, day.
        /// </summary>
        public int Value { get; }

        /// <summary>
        ///     Returns the list of mail objects in the object
        /// </summary>
        /// <returns></returns>
        public List<DatabaseData> ReturnMail();
    }

    /// <summary>
    ///     Year object that stores integer value representing a year, and a list of months.
    /// </summary>
    internal class Year : IItem {
        public readonly Month[] Months = new Month[12];
        public Year(int value) => Value = value;
        public int Value { get; }

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
            var searchMonth = Months[month - 1];
            if (searchMonth is null)
                Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month - 1] = new Month(this, month);
            Months[month - 1].AddMail(mail);
        }

        public bool RemoveMail(DatabaseData mail) {
            var searchMonth = Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month - 1];
            return searchMonth is not null && searchMonth.RemoveMail(mail);
        }

        public DatabaseData? FindMail(DatabaseData mail) =>
            Months[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Month - 1].FindMail(mail);
    }

    internal class Month : IItem {
        public readonly Day[] Days;
        private readonly Year Year;

        public Month(Year year, int month) {
            Value = month;
            Year = year;
            Days = new Day[DateTime.DaysInMonth(Year.Value, Value)];
        }

        public int Value { get; }

        public List<DatabaseData> ReturnMail() {
            var data = new List<DatabaseData>();
            for (var j = 0; j < Days.Length; j++) {
                if (Days[j] is null) continue;
                data.AddRange(Days[j].ReturnMail());
            }

            return data;
        }

        public void AddMail(DatabaseData mail) {
            var day = DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day;
            var searchDay = Days[day - 1];
            if (searchDay is null) Days[day - 1] = new Day(day, this, new SortedList<long, DatabaseData>());
            Days[day - 1].AddMail(mail);
        }

        public bool RemoveMail(DatabaseData mail) {
            var day = DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day - 1;
            var searchDay = Days[day];
            return searchDay is not null && Days[day].RemoveMail(mail);
        }

        public DatabaseData? FindMail(DatabaseData mail) =>
            Days[DateTimeOffset.FromUnixTimeSeconds(mail.Time).Day - 1].FindMail(mail);
    }

    internal class Day : IItem {
        public SortedList<long, DatabaseData> Mail;
        public int MailCount;
        private Month Month;

        public Day(int day, Month month, SortedList<long, DatabaseData> mail) {
            Month = month;
            Value = day;
            Mail = mail;
            MailCount = mail.Count;
        }

        public int Value { get; }

        public List<DatabaseData> ReturnMail() => Mail.Values.ToList();

        public void AddMail(DatabaseData mail) {
            var containsKey = false;
            var containsValue = false;
            foreach (var item in Mail)
                if (item.Key.Equals(mail.Time))
                    containsKey = true;

            if (containsKey) {
                DatabaseData? value = null;
                foreach (var item in Mail)
                    if (item.Value.Id.Equals(mail.Id)) {
                        value = item.Value;
                        containsValue = true;
                    }

                if (value != null) {
                    var sameEntry = value.DeleteFlag.Equals(mail.DeleteFlag) && value.NewFlag.Equals(mail.NewFlag);
                    if (sameEntry)
                        return;
                    Mail[value.Time] = mail;
                }
            }

            if (containsKey is false && containsValue is false || containsKey && !containsValue) {
                if (containsKey &&
                    !containsValue) // Safeguard for the nearly impossible moment when two emails have been sent at the exact precise moment.
                    mail.Time++;
                Mail.Add(mail.Time, mail);
            }
        }

        public bool RemoveMail(DatabaseData mail) {
            var res = false;
            if (Mail.ContainsKey(mail.Time)) {
                var data = Mail[mail.Time];
                if (data.Id.Equals(mail.Id)) res = Mail.Remove(mail.Time);
                
            }
            return res;
        }

        public DatabaseData? FindMail(DatabaseData mail) {
            var data = Mail[mail.Time];
            if (data.Id.Equals(mail.Id)) return data;
            return null;
        }
    }
}