using MailKit.Net.Imap;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using MailKit;
using MailKit.Search;
using MailKit.Security;
using System.IO;

#nullable enable
namespace LLU.Android.Controllers
{
    public abstract class Email
    {
        public static string FilePath(string filename) => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), filename);
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
                if (attachment is MessagePart)
				{
					var fileName = attachment.ContentDisposition?.FileName;
					var rfc822 = (MessagePart)attachment;

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

				if (bodyPart is MessagePart)
				{
					var fileName = bodyPart.ContentDisposition?.FileName;
					var rfc822 = (MessagePart)bodyPart;

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
		public static List<MimeMessage>? GetMessages(string host, int port, string username, string password)
        {
            using ImapClient? client = Connect(host, port, username, password) ?? null;
            if (client == null)
                return null;
            var inbox = AccessInbox(client);
            client.DisconnectAsync(true);
            return inbox;
        }
        public static List<MimeMessage>? GetMessages(ImapClient? client)
        {

            var inbox = AccessInbox(client);
            client.DisconnectAsync(true);
            return inbox;
        }
        public static List<MimeMessage>? AccessInbox(ImapClient client)
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
            var client = new ImapClient ();
            try
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(host, port, SecureSocketOptions.Auto);
                client.Authenticate(username, password);
                return client;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
    }
}