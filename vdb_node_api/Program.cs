using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using vdb_node_api.Infrastructure;
using vdb_node_api.Services;

namespace vdb_node_api;

class Program
{
    static void Main(string[] args)
    {
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.Configuration
			.AddJsonFile("appsettings.json", false)
			.AddJsonFile("/run/secrets/secrets.json", true)
			.AddEnvironmentVariables()
			.Build();

		builder.Logging.AddConsole();
		builder.Logging.AddFile("/var/logs/Logs/vdb_node-{Date}.txt");

		builder.Services.AddControllers();

		builder.Services.AddSingleton<SettingsProviderService>();
		builder.Services.AddSingleton<MasterAccountsService>();
		builder.Services.AddSingleton<IpDedicationService>();

	    builder.Services.AddTransient<ApiAuthorizationMiddleware>();

		WebApplication app = builder.Build();

		app.UseCors(opts =>
		{
			opts.AllowAnyOrigin();
			opts.AllowAnyMethod();
			opts.AllowAnyHeader();
		});


		app.UseMiddleware<ApiAuthorizationMiddleware>();
		app.UseRouting();
		app.MapControllers();

		app.Run();
	}
}
