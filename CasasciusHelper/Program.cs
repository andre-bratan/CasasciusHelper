using System;
using System.IO;
using System.Reflection;
using CasasciusHelper.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CasasciusHelper;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
               .AddRazorComponents()
               .AddInteractiveServerComponents();

        // Swagger
        // Swashbuckle is not available in .NET 9 or later. For an alternative, see Overview of OpenAPI support in ASP.NET Core API apps.
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
            {
                //options.SupportNonNullableReferenceTypes();

                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Version = "v1", Title = "Casascius Helper API", Description = "",

                        //TermsOfService = new Uri("https://example.com/terms"),
                        //Contact = new OpenApiContact { Name = "Example Contact", Url = new Uri("https://example.com/contact") },
                        //License = new OpenApiLicense { Name = "Example License", Url = new Uri("https://example.com/license") }
                    }
                );

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }
        );

        builder.Services.RegisterApplicationServices(builder.Configuration);

        var app = builder.Build();

        // if (app.Environment.IsProduction())
        // {
        //     app.UseExceptionHandler("/Error");
        //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //     app.UseHsts();
        // }

        app.UseSwagger();
        app.UseSwaggerUI();

        //app.UseHttpsRedirection(); - not a good idea to use redirection for API applications (clients are not supposed to "understand" redirections)

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.ConfigureMinimalApiEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
