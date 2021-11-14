using Android.App;
using Android.Widget;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace LLU.Android.Controllers
{
    public abstract class EmailController
    {
        public static string FilePath(string filename) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        public static void SaveMimePart(MimePart attachment, string fileName)
        {
            using var stream = File.Create(FilePath(fileName));
            attachment.Content.DecodeTo(stream);
        }

        public static void SaveMimePart(MessagePart attachment, string fileName)
        {
            using var stream = File.Create(FilePath(fileName));
            attachment.Message.WriteTo(stream);
        }

        public static string[]? SaveAttachments(MimeMessage message)
        {
            var attachmentcount = message.Attachments.Count();
            if (attachmentcount == 0)
                return null;

            string[] filepaths = new string[attachmentcount];
            var i = 0;
            foreach (var attachment in message.Attachments)
            {
                var filepath = string.Empty;
                if (attachment is MessagePart part1)
                {
                    var fileName = attachment.ContentDisposition?.FileName;
                    var rfc822 = part1;
                    if (string.IsNullOrEmpty(fileName))
                        fileName = "attached-message.eml";
                    filepath = FilePath(fileName);
                    using var stream = File.Create(filepath);
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;
                    var fileName = part.FileName;
                    filepath = FilePath(fileName);
                    using var stream = File.Create(filepath);
                    part.Content.DecodeTo(stream);
                }
                filepaths[i] = filepath;
                i++;
            }
            return filepaths;
        }
        public static void SaveBodyParts(MimeMessage message)
        {
            foreach (var bodyPart in message.BodyParts)
            {
                if (!bodyPart.IsAttachment)
                    continue;

                if (bodyPart is MessagePart part1)
                {
                    var fileName = bodyPart.ContentDisposition?.FileName;
                    var rfc822 = part1;
                    if (string.IsNullOrEmpty(fileName))
                        fileName = "attached-message.eml";
                    using var stream = File.Create(FilePath(fileName));
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)bodyPart;
                    var fileName = part.FileName;

                    using var stream = File.Create(FilePath(fileName));
                    part.Content.DecodeTo(stream);
                }
            }
        }
        //Get Messages by using an existing client.
        //Disconnect on finish
        public static List<MimeMessage> GetMessages(string host, int port, string username, string password)
        {
            using ImapClient? client = Connect(host, port, username, password) ?? null;
            if (client == null)
                return new();
            var inbox = AccessMessages(client);
            if (inbox == null) return new();
            return inbox;
        }
        public static List<MimeMessage> GetMessages(ImapClient client)
        {

            var inbox = AccessMessages(client);
            if(inbox == null) return new();
            return inbox;
        }
        public static bool DeleteMessages(string host, int port, string username, string password, List<string> Ids)
        {
            using var client = Connect(host, port, username, password);
            if (client == null)
                return false;
            try
            {
                foreach (var id in Ids)
                {
                    client.Inbox.AddFlagsAsync(UniqueId.Parse(id), MessageFlags.Deleted, true);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
            
        }
        public static List<MimeMessage>? AccessMessages(ImapClient client)
        {
            List<MimeMessage> messages = new();
            var state = client.Inbox.Open(FolderAccess.ReadOnly);
            if (state != FolderAccess.None)
            {
                var uids = client.Inbox.Search(SearchQuery.All);
                messages = uids.Select(uid => client.Inbox.GetMessage(uid)).ToList();
            }
            return messages;
        }

        //Create a new connection with the server.
        //Authentificate and return the client.
        //Instead of crashing the app on failure, give out null, to implement safeguards and fallbacks in-app code. 
        public static ImapClient? Connect(string host, int port, string username, string password)
        {
            var client = new ImapClient();
            try
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(host, port, SecureSocketOptions.Auto);
                client.Authenticate(username, password);
                return client;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Toast.MakeText(Application.Context,"Savienojums ar serveri neizdevās!",ToastLength.Short);
                return null;
            }
        }
    }
}