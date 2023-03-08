using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using vdb_node_api.Infrastructure;
using vdb_node_api.Services;

#if DEBUG
using Microsoft.OpenApi.Models;
#endif

namespace vdb_node_api;

class Program
{
	static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.Configuration
			.AddJsonFile("./appsettings.json", true)
			.AddJsonFile("/run/secrets/aspsecrets.json", true)
			.AddEnvironmentVariables()
			.Build();

		builder.Logging.AddConsole();

		builder.Services.AddControllers();

#if DEBUG
		if (builder.Environment.IsDevelopment())
		{
			builder.Services.AddSwaggerGen(c =>
			{
				//c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestSwagger", Version = "v1" });
				c.CustomSchemaIds(x => x.FullName);
			});
		}
#endif

		builder.Services.AddSingleton<EnvironmentProvider>();
		builder.Services.AddSingleton<SettingsProviderService>();
		builder.Services.AddSingleton<MasterAccountsService>();
		builder.Services.AddSingleton<IpDedicationService>();
		builder.Services.AddSingleton<PeersBackgroundService>();
		builder.Services.AddHostedService(pr => pr.GetRequiredService<PeersBackgroundService>());

		builder.Services.AddTransient<ApiAuthorizationMiddleware>();

		builder.WebHost.UseKestrel(opts => opts.ListenAnyIP(5000));

		WebApplication app = builder.Build();


		app.UseCors(opts =>
		{
			opts.AllowAnyOrigin();
			opts.AllowAnyMethod();
			opts.AllowAnyHeader();
		});

		var envProvider = app.Services.GetRequiredService<EnvironmentProvider>();
		if (!envProvider.ALLOW_NOAUTH)
		{
			app.UseMiddleware<ApiAuthorizationMiddleware>();
		}

		app.UseRouting();
		app.MapControllers();

#if DEBUG
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
#endif

		app.Run();
	}
}
