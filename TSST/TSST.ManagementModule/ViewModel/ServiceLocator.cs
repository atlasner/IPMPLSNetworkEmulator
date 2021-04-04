using Microsoft.Extensions.DependencyInjection;
using System;
using TSST.ManagementModule.Service.ConfigReaderService;
using TSST.ManagementModule.Service.ManagementService;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.ManagementModule.ViewModel
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
                new ConfigReaderService(args.Length == 2 ? args[1] : "config/ManagementConfig.txt"));


            services.AddSingleton<IManagementService, ManagementService>();
            services.AddSingleton<MainViewModel>();

        }

        public MainViewModel Main => _serviceProvider.GetService<MainViewModel>();
    }
}