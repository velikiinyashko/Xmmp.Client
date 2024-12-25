using XmppDotNet;

namespace Xmpp.Client.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var client = ClientExtension.Get();
            await client.Connect(conf =>
            {
                conf.User = "test";
                conf.Password = "test";
                conf.Domain = "localhost";
            });

            while (true)
            {
                await client.SendMessage("test2@localhost", $"test message id #{Guid.NewGuid()}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            Assert.True(true);
        }
    }
}