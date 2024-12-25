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
                conf.User = "test_test2";
                conf.Password = "1li1hyf";
                conf.Domain = "79.141.65.66";
            });

            while (true)
            {
                await client.SendMessage("test_test2", $"test message id #{Guid.NewGuid()}");
            }

            Assert.True(true);
        }
    }
}