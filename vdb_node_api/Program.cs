using vdb_node_api.Infrastructure;
using vdb_node_api.Services;

#if DEBUG
#endif

namespace vdb_node_api;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

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

		var app = builder.Build();


		app.UseCors(opts =>
		{
			opts.AllowAnyOrigin();
			opts.AllowAnyMethod();
			opts.AllowAnyHeader();
		});

#if DEBUG
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
#endif

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
