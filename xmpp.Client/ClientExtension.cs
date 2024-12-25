namespace Xmpp.Client;

public static class ClientExtension
{
    public static IXmppClient Get()
    {
        return new Client();
    }
}