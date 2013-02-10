using System.Web;
using HtmlAgilityPack;

namespace Errordite.Core.Notifications.Sending
{
    public class ExtractSubjectMessageSenderWrapper : IMessageSender
    {
        private readonly IMessageSender _worker;

        public ExtractSubjectMessageSenderWrapper(IMessageSender worker)
        {
            _worker = worker;
        }

        public void Send(Message message)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(message.Body);

            var titleNode = doc.DocumentNode.SelectSingleNode("//title");

            if (titleNode != null)
                message.Subject = HttpUtility.HtmlDecode(titleNode.InnerText.Trim());

            _worker.Send(message);
        }
    }
}