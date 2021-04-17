using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NHiLo.Example
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
            var serviceCollection = ConfigureServices(new ServiceCollection());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
            
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<ITextService>(provider => new TextService("Hi WPF .NET Core 3.0!"));
            services.AddSingleton<MainWindow>();
            return services;
        }
    }
}
