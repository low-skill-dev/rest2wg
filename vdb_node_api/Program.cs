using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using vdb_node_api.Infrastructure.Database;

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

		builder.Host.ConfigureLogging(opts =>
		{
			opts.AddConsole();
		});

		builder.Services.AddDbContext<VdbNodeContext>(opts =>
		{
			opts.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]);
		});

		builder.Services.AddAuthentication(opts =>
		{
			opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(opts =>
		{
			opts.RequireHttpsMetadata = false;
			opts.SaveToken = false;
			opts.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtServiceSettings:signingKey"])),
				ValidateAudience = false,
				ValidateIssuer = false,
			};
		} //  AddIdentityServerJwt
		).AddIdentityServerJwt();

		builder.Services.AddControllers();

		if (builder.Environment.IsDevelopment())
		{
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(opts =>
			{
				opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "Please insert JWT with Bearer into field",
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT"
				});
				opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					new string[] { }
				}});
			});
			builder.Services.AddDirectoryBrowser();
		}




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


		app.UseAuthentication();
		app.UseRouting();
		app.UseAuthorization();

		app.MapControllers();

		app.Run();
	}
}
