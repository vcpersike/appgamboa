using AppGamboa.Services;
using AppGamboa.Shared.Services;
using AppGamboa.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace AppGamboa
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the AppGamboa.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddSingleton<IProjectService, ProjectService>();
            builder.Services.AddSingleton<IContactService, ContactService>();

            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<ContactViewModel>();
            builder.Services.AddSingleton<ProjectsViewModel>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
