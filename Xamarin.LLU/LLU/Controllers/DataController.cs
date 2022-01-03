using System;
using System.IO;
using System.Linq;
using LLU.Models;
using MailKit;
using MimeKit;
using MimeKit.Text;

namespace LLU.Android.Controllers;

internal static class DataController {
    private static string GetLocalAppData() =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string GetFilePath(string filename) => Path.Combine(GetLocalAppData(), filename);

    public static DatabaseData ConvertFromMime(MimeMessage item, UniqueId id, string folder, bool hasRead,
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

    public static MimeMessage? CreateEmail(string receiversString, string sender, string? subject, string? body) {
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

    public static void SaveMimePart(MimePart attachment, string fileName) {
        using var stream = File.Create(GetFilePath(fileName));
        attachment.Content.DecodeTo(stream);
    }

    public static void SaveMimePart(MessagePart attachment, string fileName) {
        using var stream = File.Create(GetFilePath(fileName));
        attachment.Message.WriteTo(stream);
    }

    public static string[] SaveAttachments(MimeMessage message) {
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

    public static void SaveBodyParts(MimeMessage message) {
        foreach (var bodyPart in message.BodyParts) {
            if (!bodyPart.IsAttachment)
                continue;

            if (bodyPart is MessagePart part1) {
                var fileName = bodyPart.ContentDisposition?.FileName;
                var rfc822 = part1;
                if (string.IsNullOrEmpty(fileName))
                    fileName = "attached-message.eml";
                using var stream = File.Create(GetFilePath(fileName));
                rfc822.Message.WriteTo(stream);
            }
            else {
                var part = (MimePart) bodyPart;
                var fileName = part.FileName;

                using var stream = File.Create(GetFilePath(fileName));
                part.Content.DecodeTo(stream);
            }
        }
    }
}