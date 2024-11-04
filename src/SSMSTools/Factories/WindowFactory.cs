using Microsoft.Extensions.DependencyInjection;
using SSMSTools.Factories.Interfaces;
using System;

namespace SSMSTools.Factories
{
    internal class WindowFactory: IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateWindow<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
