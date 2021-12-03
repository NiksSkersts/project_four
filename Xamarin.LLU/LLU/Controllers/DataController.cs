using MimeKit;
using System.IO;
using System.Linq;

namespace LLU.Android.Controllers
{
    internal static class DataController
    {
        public static string GetLocalAppData() => System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        public static string GetFilePath(string filename) => Path.Combine(GetLocalAppData(), filename);
        public static void SaveMimePart(MimePart attachment, string fileName)
        {
            using var stream = File.Create(GetFilePath(fileName));
            attachment.Content.DecodeTo(stream);
        }

        public static void SaveMimePart(MessagePart attachment, string fileName)
        {
            using var stream = File.Create(GetFilePath(fileName));
            attachment.Message.WriteTo(stream);
        }
        public static string[] SaveAttachments(MimeMessage message)
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
                    filepath = GetFilePath(fileName);
                    using var stream = File.Create(filepath);
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;
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
                    using var stream = File.Create(GetFilePath(fileName));
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)bodyPart;
                    var fileName = part.FileName;

                    using var stream = File.Create(GetFilePath(fileName));
                    part.Content.DecodeTo(stream);
                }
            }
        }
    }
}