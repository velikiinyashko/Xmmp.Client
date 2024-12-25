using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using XmppDotNet;
using XmppDotNet.Xmpp.Client;

namespace Xmpp.Client
{
    public interface IXmppClient
    {
        public ISubject<string> MessageHandler { get; set; }
        public SessionState State { get; set; }
        public Task Connect(Action<Configuration> config);
        public Task SendMessage(string contact, string msg);
        public Task<Iq> GetVCard(string contact);
    }
}