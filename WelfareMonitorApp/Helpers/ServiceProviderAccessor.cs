// Helpers/ServiceProviderAccessor.cs
using System;

namespace WelfareMonitorApp.Helpers
{
    public static class ServiceProviderAccessor
    {
        public static IServiceProvider Instance { get; set; }

        public static T GetService<T>() => (T)Instance.GetService(typeof(T));
    }
}
