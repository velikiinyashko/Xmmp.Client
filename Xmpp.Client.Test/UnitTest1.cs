using System.Diagnostics;

namespace Xmpp.Client.Test
{
    using Configuration = Xmpp.Client.v2.Configuration;

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        private Action<Configuration> local = conf =>
        {
            conf.User = "test";
            conf.Password = "test";
            conf.Domain = "localhost";
        };

        private Action<Configuration> gz = conf =>
        {
            conf.User = "krs_d2";
            conf.Password = "0nA7yW19";
            conf.Domain = "openfire.garzdrav.ru";
        };

        [Test]
        public async Task Test1()
        {
            var client = Xmpp.Client.v2.ClientExtension.Get();
            await client.Connect(local);


            client.MessageHandler.Subscribe(msg => { Debug.WriteLine(msg); });

            while (true)
            {
                // await client.SendMessage("test2@localhost", $"test message id #{Guid.NewGuid()}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            Assert.Pass("Ok");
        }
    }
}