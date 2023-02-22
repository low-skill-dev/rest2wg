using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Text;

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

		builder.Services.AddControllers();

		WebApplication app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseCors(opts =>
		{
			opts.AllowAnyOrigin();
			opts.AllowAnyMethod();
			opts.AllowAnyHeader();
		});


		app.UseMiddleware<AuthorizationMiddleware>();
		app.UseRouting();
		app.MapControllers();

		app.Run();
	}
}
