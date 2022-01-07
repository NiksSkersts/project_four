using System;
using System.IO;
using System.Linq;
using LLU.Models;
using MailKit;
using MimeKit;
using MimeKit.Text;

namespace LLU.Android.Controllers;

internal static partial class DataController {
    /// <summary>
    ///     Emails received from the server are received in MimeMessage format. To be able to put them in database, they get
    ///     converted to a custom class
    ///     "DatabaseData".
    /// </summary>
    /// <param name="item">E-mail from the server.</param>
    /// <param name="id">Each e-mail has an unique ID for each folder.</param>
    /// <param name="folder">Folder where the message is stored in. Mind the UniqueID part.</param>
    /// <param name="hasRead">Has the e-mail been read?</param>
    /// <param name="hasBeenDeleted">Has the e-mail been deleted?</param>
    /// <returns></returns>
    public static partial DatabaseData ConvertFromMime(MimeMessage item, UniqueId id, string folder, bool hasRead,
        bool hasBeenDeleted);

    /// <summary>
    ///     Creates an e-mail by creating a new MimeMessage.
    /// </summary>
    /// <param name="receiversString"> All the emails which are supposed to receive the email.</param>
    /// <param name="sender">The one who sends it.</param>
    /// <param name="subject">Subject of the email.</param>
    /// <param name="body">Text, HTMl formatted body.</param>
    /// <returns>MimeMessage or null, if the creation failed.</returns>
    public static partial MimeMessage?
        CreateEmail(string receiversString, string sender, string? subject, string? body);

    public static partial string GetFilePath(string filename);
    private static partial string GetLocalAppData();

    /// <summary>
    ///     Saves all the attachments that are handed alongside the MimeMessage.
    /// </summary>
    /// <returns>List of locations where the files have been stored at.</returns>
    public static partial string[] SaveAttachments(MimeMessage message);
}

internal static partial class DataController {
    private static partial string GetLocalAppData() =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static partial string GetFilePath(string filename) => Path.Combine(GetLocalAppData(), filename);

    public static partial DatabaseData ConvertFromMime(MimeMessage item, UniqueId id, string folder, bool hasRead,
        bool hasBeenDeleted) {
        DatabaseData dataTemp = new() {
            UniqueId = id.ToString(),
            Folder = folder,
            From = item.From.ToString(),
            To = item.To.ToString(),
            Subject = item.Subject,
            Time = item.Date.ToUnixTimeSeconds(),
            DeleteFlag = hasBeenDeleted,
            NewFlag = hasRead
        };
        if (item.HtmlBody != null) {
            dataTemp.Body = item.HtmlBody;
            dataTemp.IsHtmlBody = true;
        }
        else {
            dataTemp.Body = item.TextBody;
        }

        if (item.Priority == MessagePriority.Urgent ||
            item.XPriority is XMessagePriority.High or XMessagePriority.Highest)
            dataTemp.PriorityFlag = true;
        else
            dataTemp.PriorityFlag = false;
        dataTemp.Id = item.MessageId ?? dataTemp.UniqueId;
        return dataTemp;
    }

    public static partial MimeMessage?
        CreateEmail(string receiversString, string sender, string? subject, string? body) {
        subject ??= string.Empty;
        body ??= string.Empty;


        var email = new MimeMessage();
        var receivers = receiversString.Split(';');
        var fullList = receivers.Where(mailaddress => mailaddress.Contains('@')).ToList();
        for (var i = 0; i < fullList.Count - 1; i++)
            fullList[i] = fullList[i].Trim();
        try {
            email.From.Add(MailboxAddress.Parse($"{sender}@llu.lv"));
            foreach (var receiver in fullList)
                email.To.Add(MailboxAddress.Parse(receiver));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Text) {Text = body};
        }
        catch {
            return null;
        }

        return email;
    }

    public static partial string[] SaveAttachments(MimeMessage message) {
        var attachmentcount = message.Attachments.Count();
        if (attachmentcount == 0)
            return null;

        var filepaths = new string[attachmentcount];
        var i = 0;
        foreach (var attachment in message.Attachments) {
            var filepath = string.Empty;
            if (attachment is MessagePart part1) {
                var fileName = attachment.ContentDisposition?.FileName;
                var rfc822 = part1;
                if (string.IsNullOrEmpty(fileName))
                    fileName = "attached-message.eml";
                filepath = GetFilePath(fileName);
                using var stream = File.Create(filepath);
                rfc822.Message.WriteTo(stream);
            }
            else {
                var part = (MimePart) attachment;
                var fileName = part.FileName;
                filepath = GetFilePath(fileName);
                using var stream = File.Create(filepath);
                part.Content.DecodeTo(stream);
            }

            filepaths[i] = filepath;
            i++;
        }

        return filepaths;
    }
}