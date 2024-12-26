namespace Xmpp.Client.v2;

public static class ClientExtension
{
    public static IXmppClient Get()
    {
        return new Client();
    }
}