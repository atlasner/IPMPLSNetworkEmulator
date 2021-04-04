using System;
using Microsoft.Extensions.DependencyInjection;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;
using TSST.Subnetwork.Service.CCService;
using TSST.Subnetwork.Service.ConfigReaderService;
using TSST.Subnetwork.Service.ConnectivityService;
using TSST.Subnetwork.Service.RCService;

namespace TSST.Subnetwork.ViewModel
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
                new ConfigReaderService(args.Length == 2 ? args[1] : "config/SubnetworkConfig.txt"));


            services.AddSingleton<ICableCloudConnectionService, CableCloudConnectionService>();
            services.AddSingleton<ICCService, CCService>();
            services.AddSingleton<IRCService, RCService>();
            services.AddSingleton<IConnectivityService, ConnectivityService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<RCViewModel>();

        }

        public MainViewModel Main => _serviceProvider.GetService<MainViewModel>();
        public RCViewModel RC => _serviceProvider.GetService<RCViewModel>();
    }
}