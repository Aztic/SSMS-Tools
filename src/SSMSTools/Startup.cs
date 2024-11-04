﻿using EnvDTE;
using EnvDTE80;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using SSMSTools.Factories;
using SSMSTools.Factories.Interfaces;
using SSMSTools.Managers;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Windows.Interfaces;
using SSMSTools.Windows.MultiDbQueryRunner;
using System;

namespace SSMSTools
{
    internal class Startup
    {
        private readonly AsyncPackage _package;

        public Startup(AsyncPackage package)
        {
            _package = package;
        }

        public IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            RegisterManagers(serviceCollection);
            RegisterServices(serviceCollection);
            RegisterWindows(serviceCollection);
            RegisterFactories(serviceCollection);

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private void RegisterManagers(IServiceCollection services)
        {
            services.AddTransient<IMessageManager, MessageManager>();
        }

        private void RegisterServices(IServiceCollection services)
        {
            // Register DTE2
            services.AddTransient(provider =>
            {
                return _package.GetServiceAsync(typeof(DTE)).Result as DTE2;
            });

            // Register IObjectExplorerService
            services.AddTransient(provider =>
            {
                return _package.GetServiceAsync(typeof(IObjectExplorerService)).Result as IObjectExplorerService;
            });
        }

        private void RegisterWindows(IServiceCollection services)
        {
            services.AddTransient<IMultiDbQueryRunnerWIndow, MultiDbQueryRunnerWindow>();
        }

        private void RegisterFactories(IServiceCollection services)
        {
            services.AddTransient<IWindowFactory, WindowFactory>();
        }
    }
}
