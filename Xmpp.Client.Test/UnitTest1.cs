using System.Diagnostics;

namespace Xmpp.Client.Test
{
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
            var client = ClientExtension.Get();
            await client.Connect(gz);


            client.MessageHandler.Subscribe(msg => { Debug.WriteLine(msg); });

            while (true)
            {
                await client.SendMessage("test_test2@openfire", $"test message id #{Guid.NewGuid()}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            Assert.Pass("Ok");
        }
    }
}