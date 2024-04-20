using Api.Infrastructure;
using Api.Services;

#if DEBUG
#endif

namespace Api;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Configuration
			.AddJsonFile("./appsettings.json", true)
			.AddJsonFile("/run/secrets/secrets.json", true)
			.AddEnvironmentVariables()
			.Build();

		builder.Logging.AddConsole();

		builder.Services.AddControllers();


		builder.Services.AddSingleton<EnvironmentProvider>();
		builder.Services.AddSingleton<SettingsProviderService>();
		builder.Services.AddSingleton<MasterAccountsService>();
		builder.Services.AddSingleton<IpDedicationService>();
		builder.Services.AddSingleton<PeersBackgroundService>();
		builder.Services.AddHostedService(pr => pr.GetRequiredService<PeersBackgroundService>());

		builder.Services.AddTransient<ApiAuthorizationMiddleware>();

		var app = builder.Build();


		var envProvider = app.Services.GetRequiredService<EnvironmentProvider>();
		if (!(envProvider.ALLOW_NOAUTH ?? false))
		{
			app.UseMiddleware<ApiAuthorizationMiddleware>();
		}

		app.UseRouting();
		app.MapControllers();

		app.Run();
	}
}
