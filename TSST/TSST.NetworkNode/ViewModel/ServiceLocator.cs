using Microsoft.Extensions.DependencyInjection;
using System;
using TSST.NetworkNode.Service.ConfigReaderService;
using TSST.NetworkNode.Service.LRMService;
using TSST.NetworkNode.Service.ManagementAgentService;
using TSST.NetworkNode.Service.RoutingService;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.NetworkNode.ViewModel
{
    class ServiceLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceLocator()
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogService, LogService>();
            services.AddTransient<IObjectSerializerService, ObjectSerializerService>();

            var args = Environment.GetCommandLineArgs();
            services.AddTransient<IConfigReaderService>(_ =>
                new ConfigReaderService(args.Length == 2 ? args[1] : "config/NetworkNodeConfig.txt"));


            services.AddSingleton<ICableCloudConnectionService, CableCloudConnectionService>();
            services.AddSingleton<IRoutingService, RoutingService>();
            services.AddSingleton<IManagementAgentService, ManagementAgentService>();
            services.AddSingleton<ILRMService, LRMService>();

            services.AddSingleton<MainViewModel>();

        }

        public MainViewModel Main => _serviceProvider.GetService<MainViewModel>();
    }
}
