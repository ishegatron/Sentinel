namespace Sentinel.Log4Net
{
    using Interfaces.Providers;

    public interface IUdpAppenderListenerSettings : IProviderSettings
    {
        int Port { get; set; }
    }
}