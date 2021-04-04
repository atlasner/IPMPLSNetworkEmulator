using System;
using Microsoft.Extensions.DependencyInjection;
using TSST.NCC.Service.ConfigReaderService;
using TSST.NCC.Service.NCCService;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.NCC.ViewModel
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
                new ConfigReaderService(args.Length == 2 ? args[1] : "config/NCCConfig.txt"));


            services.AddSingleton<ICableCloudConnectionService, CableCloudConnectionService>();
            services.AddSingleton<INccService, NccService>();

            services.AddSingleton<MainViewModel>();

        }

        public MainViewModel Main => _serviceProvider.GetService<MainViewModel>();
    }
}