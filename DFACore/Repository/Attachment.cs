using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class Attachment
    {
        public Attachment(string fileName, Stream stream, ContentType contentType)
        {
            MyStream = stream;
            FileName = fileName;
            //Type = AttachmentType.Text;
            MyContentType = contentType;
        }

        public enum AttachmentType
        {
            Json,
            Text
        }

        public ContentType MyContentType { get; set; }
        public Stream MyStream { get; set; }
        public object Content { get; set; }
        public string FileName { get; set; }
        public AttachmentType Type { get; set; }

        public async Task<MemoryStream> ContentToStreamAsync()
        {
            string text;
            switch (Type)
            {
                case AttachmentType.Json:
                    text = Newtonsoft.Json.JsonConvert.SerializeObject(Content);
                    break;
                case AttachmentType.Text:
                    text = Content.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Would create a string extension method to be able to reuse the following.
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(text);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }
    }
}
