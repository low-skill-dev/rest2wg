using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using vdb_node_api.Infrastructure.Database;
using vdb_node_api.Models.Api.Application;
using vdb_node_api.Models.Api.Master;
using vdb_node_api.Models.Database;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers
{
	/* Итак, после проведения минимального прототипирования и наброски кода, 
	 * были приняты некоторые архитектурные решения. Для приложения определяются 
	 * некоторые мастер-аккаунты, которые записываются в файл секретов и передаются 
	 * приложению в момент развертывания. Данные аккаунты имеют доступ к созданию 
	 * обычных аккаунтов – которые непосредственно манипулируют Wireguard-сервисом. 
	 * Во время исполнения невозможно добавление мастер-аккаунтов, но возможна их 
	 * блокировка до перезапуска приложения. Каждый мастер-аккаунт от своего имени 
	 * может заблокировать либо только себя, либо все мастер-аккаунты. Обычные аккаунты 
	 * хранятся в качестве записей в базе данных.
	 */

	[AllowAnonymous]
	[ApiController]
	[Route("{controller}")]
	public sealed class ApplicationsController : ControllerBase
	{
		private readonly ILogger<ApplicationsController> _logger;
		private readonly VdbNodeContext _context;
		private readonly JwtService _jwtService;
		private readonly ApplicationAccountService _accountService;
		private readonly MasterAccountService _mastersService;

		public ApplicationsController(
			JwtService jwtService, ApplicationAccountService accountService,
			MasterAccountService mastersService, VdbNodeContext vdbNodeContext,
			ILogger<ApplicationsController> logger)
		{
			_jwtService = jwtService;
			_accountService = accountService;
			_mastersService = mastersService;
			_context = vdbNodeContext;
			_logger = logger;
		}
		/// <returns>
		/// ApplicationAccount if apiKey was found in database,<br/>
		/// null otherwise.
		/// </returns>
		[NonAction]
		private async Task<ApplicationAccount?> findAppByKey(string? apiKey)
		{
			if (string.IsNullOrEmpty(apiKey)) return null;

			var keyHash = Base64Url.Encode(SHA512.HashData(Base64Url.Decode(apiKey)));
			return await _context.ApplicationAccounts.FirstOrDefaultAsync(x => x.ApiKeyHash.Equals(keyHash));
		}

		/* Метод служит для валидации JWT токена пользователя API.
		 * Данный метод не выполняет реальной работы, а лишь подтверждает, 
		 * что мидлваре, отвечающее за авторизацию, пропустило запрос.
		 * При том метод не осуществляет фактическую проверку действительности
		 * ключа, это должно произойти при смерти краткосрочного JWT.
		 */
		[HttpGet, Authorize]
		[Route("validate")]
		public async Task<IActionResult> ValidateApplicationJwt()
		{
			// I hate warnings, I hate warnings, I hate warnings... I HATE THIS GREEN LINES I HATE WARNINGS !!!!!!!
			return await Task.FromResult(Ok());
		}

		/* Метод служит для получения JWT токена пользователя API.
		 */
		[HttpPost, AllowAnonymous]
		[Route("login")]
		public async Task<IActionResult> LoginApplication([Required][FromBody] LoginRequest request)
		{
			var found = await findAppByKey(request.ApiKey);
			if (found is null || !found.CanAccessNow)
			{
				return Unauthorized();
			}

			var generatedJwt = _jwtService.GenerateJwtToken(found);
			return Ok(new LoginResponse(generatedJwt));
		}

		/* Метод служит для обновления API-ключа приложения на основании действительного.
		 * Данная реализация служит заделом под механику refresh-ключей. Добавление в БД
		 * одного поля NotAfter может запретить работу ключа, но разрешить её в данном методе.
		 */
		[HttpPatch, AllowAnonymous]
		[Route("renew-key")]
		public async Task<IActionResult> RenewApplicationKey([Required][FromBody] LoginRequest request)
		{
			var found = await findAppByKey(request.ApiKey);
			if (found is null || !found.CanRefreshNow)
			{
				return Unauthorized();
			}

			var newKey = _accountService.RefreshAccountEntityKey(_context.Entry(found));
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Application key was updated: " +
				$"\'{request.ApiKey}\' (original key) -> " +
				$"\'{newKey.Substring(newKey.Length / 32)}...\'");
			return Ok(new RenewApiKeyResponse(newKey));			
		}

		[HttpPut, AllowAnonymous]
		[Route("by-master/generate")]
		public async Task<IActionResult> GenerateNewAccount([Required][FromBody] AppRegistrationRequest request)
		{
			if (!_mastersService.IsValid(request.MasterKey))
			{
				return Unauthorized();
			}

			var result = await _accountService.CreateNewAccount(request.NewAppName);
			return Ok(new AppRegistrationResponse(result.Item1));
		}

		[HttpGet, AllowAnonymous]
		[Route("by-master/get-info")]
		public async Task<IActionResult> GenerateNewAccount([Required][FromBody] AppRegistrationRequest request)
		{
			if (!_mastersService.IsValid(request.MasterKey))
			{
				return Unauthorized();
			}

			var result = await _accountService.CreateNewAccount(request.NewAppName);
			return Ok(new AppRegistrationResponse(result.Item1));
		}
	}
}
