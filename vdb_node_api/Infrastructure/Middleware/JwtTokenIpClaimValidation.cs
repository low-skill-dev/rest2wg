using vdb_node_api.Models.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace vdb_node_api.Infrastructure.Middleware
{
	/// <summary>
	/// This middleware may be added only after 
	/// <see cref="Microsoft.Extensions.DependencyInjection.JwtBearerExtensions.AddJwtBearer"/>
	/// .
	/// </summary>
	public class JwtTokenIpClaimValidation:IMiddleware
	{
		public Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			// no ip claim, pass any
			var ipClaim = context.User.Claims.FirstOrDefault(x => x.ValueType.Equals(UserIpJwtClaim.ValueType));
			if(ipClaim is null) return next(context);

			// there is ip claim, but request ip is not detected
			var requestAddress = context.Connection.RemoteIpAddress;
			if (requestAddress is null) 
				return Task.FromResult(new StatusCodeResult(StatusCodes.Status401Unauthorized));

			// the ip claim does not match request ip
			if (!requestAddress.Equals(System.Net.IPAddress.Parse(ipClaim.Value)))
				return Task.FromResult(new StatusCodeResult(StatusCodes.Status401Unauthorized));
			else
				return next(context);
		}
	}
}
