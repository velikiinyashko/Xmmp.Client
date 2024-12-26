using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Security.Authentication;
using System.Threading.Tasks;
using Matrix.Net;
using Matrix.Xmpp;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.Sasl;


namespace Xmpp.Client;

internal class Client : IXmppClient, IDisposable
{
    private XmppClient _client = new();
    private readonly CompositeDisposable _disposable = new();
    public ISubject<string> MessageHandler { get; set; }

    public Client()
    {
        MessageHandler = new Subject<string>();
    }


    public async Task Connect(Action<Configuration> config)
    {
        var timer = new Stopwatch();
        timer.Start();

        Configuration configuration = new Configuration();
        config(configuration);

        _client.SetUsername(configuration.User);
        _client.SetXmppDomain(configuration.Domain);
        _client.Password = configuration.Password;

        _client.Status = "I'm here";
        _client.StartTls = true;
        _client.AutoReplyToPing = true;
        _client.PreferredSsoSaslMechanism = SaslMechanism.Plain;
        _client.Transport = Transport.Socket;
        _client.TlsProtocols = SslProtocols.Tls12;
        _client.OnValidateCertificate += (sender, e) => { e.AcceptCertificate = true; };

        _client.OnPresence += (Sender, e) =>
        {
            Debug.WriteLine($"OnPresence from {e.Presence.From}");
            Debug.WriteLine($"Status {e.Presence.Status}");
            Debug.WriteLine($"Show type {e.Presence.Show}");
            Debug.WriteLine($"Priority {e.Presence.Priority}");
        };

        _client.OnMessage += (sender, e) =>
        {
            Debug.WriteLine($"OnMessage from {e.Message.From}");
            Debug.WriteLine($"Body {e.Message.Body}");
            Debug.WriteLine($"Type {e.Message.Type}");
        };

        _client.Open();
    }

    public async Task SendMessage(string contact, string msg)
    {
        try
        {
            _client.Send(new Message()
            {
                Type = MessageType.Chat,
                To = contact,
                Body = msg
            });
        }
        catch (Exception ex)
        {
            //
            throw;
        }
    }

    public async Task<Iq> GetVCard(string contact)
    {
        try
        {
            var request = new VcardIq { Type = IqType.Get, To = contact };
            var result = _client.Send(request);


            return null;
        }
        catch (Exception ex)
        {
            //
            throw;
        }
    }

    public void Dispose()
    {
        _client.Close();
        _client.Dispose();
        _disposable?.Dispose();
    }
}