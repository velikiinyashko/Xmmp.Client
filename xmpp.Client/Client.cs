using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Matrix;
using Matrix.Extensions.Client.Message;
using Matrix.Extensions.Client.Roster;
using Matrix.Network;
using Matrix.Xmpp;
using Matrix.Xmpp.Client;


namespace Xmpp.Client;

internal class Client : IXmppClient, IDisposable
{
    private XmppClient _client;
    private readonly CompositeDisposable _disposable = new();
    public ISubject<string> MessageHandler { get; set; }
    public SessionState State { get; set; }

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

        _client = new XmppClient
        {
            Username = configuration.User,
            Password = configuration.Password,
            XmppDomain = configuration.Domain,
            CertificateValidator = new AlwaysAcceptCertificateValidator()
        };

        _client.XmppSessionState.Subject.Subscribe(async ss =>
        {
            Debug.WriteLine($"Status connect: {State}");
            State = ss;
        }).DisposeWith(_disposable);

        _client.XmppXElementStreamObserver.Subscribe(h => { Debug.WriteLine(h); }).DisposeWith(_disposable);

        _client.XmppXElementStreamObserver.Where(el => el is Message)
            .Subscribe(msg =>
            {
                if (msg is Message s)
                {
                    MessageHandler.OnNext(s.Body);
                }
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
            if (State < SessionState.Binded)
            {
                throw new Exception("Not connect server");
            }

            await _client.SendAsync(new Message()
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
            if (State < SessionState.Binded)
            {
                throw new Exception("Not connect server");
            }

            var request = new VcardIq { Type = IqType.Get, To = contact };
            var result = await _client.SendIqAsync(request);

            if (result.Type == IqType.Error)
            {
                throw new Exception(result.Error.Text);
            }

            if (result.Type == IqType.Result)
            {
                return request;
            }

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