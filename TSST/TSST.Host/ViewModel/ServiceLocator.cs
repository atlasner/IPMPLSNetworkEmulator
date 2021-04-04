using System;
using Microsoft.Extensions.DependencyInjection;
using TSST.Host.Service.ConfigReaderService;
using TSST.Host.Service.CPCCService;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.Host.ViewModel
{
  
    public class ServiceLocator
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
                new ConfigReaderService(args.Length == 2 ? args[1] : "config/HostConfig.txt"));


            services.AddSingleton<ICableCloudConnectionService, CableCloudConnectionService>();
            services.AddSingleton<ICPCCService, CpccService>();
            services.AddSingleton<MainViewModel>();

        }

        public MainViewModel Main => _serviceProvider.GetService<MainViewModel>();
    }
}