using Microsoft.VisualStudio.Shell;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SSMSTools.UnitTests.Utilities
{
    public class MockableAsyncPackage : SSMSToolsPackage
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        public override IServiceProvider ServiceProvider => _serviceProviderMock.Object;


        public MockableAsyncPackage(): base()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            _services[serviceType] = serviceInstance;
        }


        protected override object GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }

        protected async Task<object> GetServiceAsync(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return await Task.FromResult(service);
        }
    }
}
