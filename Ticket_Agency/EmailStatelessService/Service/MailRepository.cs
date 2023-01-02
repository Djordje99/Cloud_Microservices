using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EmailStatelessService.Service
{
    public class MailRepository
    {
        private readonly string mailServer, login, password;
        private readonly int port;
        private readonly bool ssl;

        public MailRepository(string mailServer, int port, bool ssl, string login, string password)
        {
            this.mailServer = mailServer;
            this.port = port;
            this.ssl = ssl;
            this.login = login;
            this.password = password;
        }

        public IEnumerable<string> GetUnreadMails()
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            var messages = new List<string>();

            using (var client = new ImapClient())
            {
                client.Connect(mailServer, port, ssl);

                client.Authenticate(login, password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);
                var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen));

                foreach (var uniqueId in results.UniqueIds)
                {
                    var message = inbox.GetMessage(uniqueId);

                    htmlDocument.LoadHtml(message.HtmlBody);

                    HtmlNode body = htmlDocument.DocumentNode.SelectSingleNode("//div");

                    messages.Add(body.InnerText);

                    inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                }

                client.Disconnect(true);
            }

            return messages;
        }
    }
}
