using System.Reflection;

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.Toolkit.Maui;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using POCFederacionCafeteros.ViewModels;
using POCFederacionCafeteros.Views;

namespace POCFederacionCafeteros
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var config = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();
      var ARCGIS_API_KEY = config["ArcGISRuntime:ApiKey"];

      var builder = MauiApp.CreateBuilder();

      builder
        .UseMauiApp<App>()
        .UseArcGISRuntime(config => config.UseApiKey(ARCGIS_API_KEY ?? string.Empty))
        .UseArcGISToolkit()
        .UsePrism(prism => prism
            .RegisterTypes(registry =>
            {
              registry.RegisterForNavigation<NavigationPage>();
              registry.RegisterForNavigation<MainPage, MainPageViewModel>();
            })
            .CreateWindow(async navigationService => await navigationService.NavigateAsync("NavigationPage/MainPage")))
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

#if DEBUG
      builder.Logging.AddDebug();
#endif
      return builder.Build();
    }
  }
}
