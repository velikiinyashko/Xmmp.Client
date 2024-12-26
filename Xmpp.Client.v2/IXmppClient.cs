using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using XmppDotNet.Xmpp.Client;


namespace Xmpp.Client.v2
{
    public interface IXmppClient
    {
        public ISubject<string> MessageHandler { get; set; }
        public Task Connect(Action<Configuration> config);
        public Task SendMessage(string contact, string msg);
        public Task<Iq> GetVCard(string contact);
    }
}