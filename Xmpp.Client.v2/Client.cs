using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Message;
using XmppDotNet.Extensions.Client.Presence;
using XmppDotNet.Extensions.Client.Roster;
using XmppDotNet.Sasl;
using XmppDotNet.Transport.Socket;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;


namespace Xmpp.Client.v2;

internal class Client : IXmppClient, IDisposable
{
    private XmppClient _client;
    private readonly CompositeDisposable _disposable = new();
    public ISubject<string> MessageHandler { get; set; }
    public SessionState State { get; set; }


    public Client()
    {
        MessageHandler = new Subject<string>();
        Observable.Interval(TimeSpan.FromSeconds(10))
            .Subscribe(_ =>
            {
                if (State != SessionState.Binded)
                    return;
                _client.SendPresenceAsync(Show.DoNotDisturb, "I'm here").GetAwaiter().GetResult();
            }).DisposeWith(_disposable);
    }


    public async Task Connect(Action<Configuration> config)
    {
        var timer = new Stopwatch();
        timer.Start();

        Configuration configuration = new Configuration();
        config(configuration);

        _client = new XmppClient(conf =>
        {
            conf.AutoReconnect = true;
            conf.UseSocketTransport()
                .UseAutoReconnect()
                .WithCertificateValidator(new AlwaysAcceptCertificateValidator());
        })
        {
            Jid = $"{configuration.User}@{configuration.Domain}",
            Password = configuration.Password,
            SaslHandler = new DefaultSaslHandler(),
            Tls = true,
            Timeout = 100000,
        };

        _client.XmppXElementReceived.Subscribe(e =>
        {
            if (e is Message msg)
                MessageHandler.OnNext(msg.Body);
            Debug.WriteLine(e);
        }).DisposeWith(_disposable);

        _client.StateChanged.Subscribe(async ss =>
        {
            Debug.WriteLine($"Status connect: {State}");
            State = ss;
        }).DisposeWith(_disposable);

        await _client.ConnectAsync();

        while (true)
        {
            Debug.WriteLine($"Check status: {State}");
            if (State == SessionState.Binded)
            {
                await _client.RequestRosterAsync();
                return;
            }

            if (timer.Elapsed < TimeSpan.FromMinutes(1))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            else
            {
                await _client.DisconnectAsync();
                throw new Exception("Timeout connection");
            }
        }
    }

    public async Task SendMessage(string contact, string msg)
    {
        try
        {
            _client.SendMessageAsync(new Message()
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
            var result = _client.SendIqAsync(request);


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
        _client.DisconnectAsync().GetAwaiter().GetResult();

        _disposable?.Dispose();
    }
}