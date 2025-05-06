using AppGamboa.Shared.Services;
using AppGamboa.Shared.ViewModels;
using AppGamboa.Web.Components;
using AppGamboa.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the AppGamboa.Shared project
builder.Services.AddMudServices();
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<ContactViewModel>();
builder.Services.AddScoped<ProjectsViewModel>();

builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IProjectService, ProjectService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AppGamboa.Shared._Imports).Assembly);

app.Run();
