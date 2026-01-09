using System;
using Microsoft.Extensions.DependencyInjection;
using SharpGallery.Services;
using SharpGallery.ViewModels;

namespace SharpGallery
{
    public static class ServiceLocator
    {
        private static IServiceProvider? _serviceProvider;

        public static IServiceProvider ServiceProvider =>
            _serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been configured.");

        public static void Configure()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);
            ConfigureViewModels(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<ImageScanningService>();
        }

        private static void ConfigureViewModels(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<UpdateViewModel>();
        }

        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public static object GetService(Type serviceType)
        {
            return ServiceProvider.GetRequiredService(serviceType);
        }
    }
}
