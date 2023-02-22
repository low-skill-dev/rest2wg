using Microsoft.AspNetCore.Mvc;
using vdb_node_api.Services;
using System.Security.Cryptography;
using vdb_node_api.Models;

namespace vdb_node_api.Infrastructure
{

    public sealed class AuthorizationMiddleware : IMiddleware
    {
        private readonly MasterAccountsService _accountsService;
        public AuthorizationMiddleware(MasterAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var header = context.Request.Headers.Authorization;
            string? key;
            try
            {
                key = header.Single(); // according to RFC, the header may appear only once
            }
            catch (InvalidOperationException) // key is not single
            {
                return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
            }

            if (string.IsNullOrEmpty(key)) // key is not present
            {
                return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
            }

            try
            {
                if (_accountsService.IsValid(key)) // key format is valid, but not found
                {
                    return Task.FromResult(new StatusCodeResult(StatusCodes.Status401Unauthorized));
                }
            }
            catch // key format is invalid
            {
				return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
			}

            // key was successfully validated
            return next(context);
        }
    }
}
