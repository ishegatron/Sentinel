using System.Windows;
using IProviderInfo = Sentinel.Interfaces.Providers.IProviderInfo;

namespace Sentinel.Providers.Interfaces
{
    using Sentinel.Interfaces.Providers;

    using IProviderInfo = IProviderInfo;

    public interface INewProviderWizard
    {
        IProviderInfo Provider { get; } 

        IProviderSettings Settings { get; }

        bool Display(Window parent);
    }
}