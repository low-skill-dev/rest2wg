using vdb_node_api.Models.Database;
using vdb_node_api.Models.Runtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vdb_node_api.Models.Database;
using System.Runtime;
using System.Security.Principal;

namespace vdb_node_api.Services
{

	/* Singleton-сервис, заниющйся генерацией Jwt по набору утверждений
	 * с дальнейшей генерацией HMAC SHA512 подписи токена.
	 */
	public class JwtService
	{
		protected readonly SettingsProviderService _settingsProvider;
		protected virtual JwtServiceSettings _settings => _settingsProvider.JwtServiceSettings;
		private readonly JwtSecurityTokenHandler _tokenHandler;
		private readonly SymmetricSecurityKey _symmetricKey;
		private readonly SigningCredentials _signingCredentials;

		public JwtService(SettingsProviderService settingsProvider)
		{
			this._settingsProvider = settingsProvider;

			this._tokenHandler = new JwtSecurityTokenHandler();
			this._symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.SigningKey));
			this._signingCredentials = new SigningCredentials(this._symmetricKey, SecurityAlgorithms.HmacSha512Signature);
		}

		/* Данный метод является конкретной реализацией для данного приложения, 
		 * в то время как остальные были скопированы. 
		 */
		public string GenerateJwtToken(ApplicationAccount account)
			=> GenerateJwtToken(new Claim[] {
				new Claim(nameof(account.Id), account.Id.ToString()),
				new Claim(nameof(account.ApiKeyHash), account.ApiKeyHash ?? string.Empty),
				new Claim(nameof(account.CreatedDateTimeUtc),account.CreatedDateTimeUtc.Ticks.ToString()),
				new Claim(nameof(account.LastAccessDateTimeUtc),account.LastAccessDateTimeUtc.Ticks.ToString()),
				new Claim(nameof(account.AccessNotBeforeUtc),account.AccessNotBeforeUtc.Ticks.ToString()),
				new Claim(nameof(account.AccessNotAfterUtc),account.AccessNotAfterUtc.Ticks.ToString()),
				new Claim(nameof(account.RefreshNotBeforeUtc),account.RefreshNotBeforeUtc.Ticks.ToString()),
				new Claim(nameof(account.RefreshNotAfterUtc),account.RefreshNotAfterUtc.Ticks.ToString())
			});

		public string GenerateJwtToken(IList<Claim> claims, DateTime? expires = null)
		{
			return _tokenHandler.WriteToken(_tokenHandler.CreateToken(new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = expires ?? DateTime.UtcNow.AddDays(_settings.TokenLifespanDays),
				Issuer = _settings.Issuer,
				SigningCredentials = _signingCredentials
			}));
		}

		[Obsolete(@"Use built-in 'Authorize' attr.")] // we are using built-in 'Authorize' attr.
		public ClaimsPrincipal ValidateJwtToken(string token)
		{
			var result = _tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _settings.Issuer,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _symmetricKey
#if DEBUG
				,
				/* Данный твик устанавливает шаг проверки валидации времени смерти токена.
				 * https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/dev/src/Microsoft.IdentityModel.Tokens/TokenValidationParameters.cs#L339
				 * По умолчанию 5 минут, для тестов это слишком долго.
				 */
				ClockSkew = TimeSpan.Zero
#endif
			}, out _);
			return result;
		}
	}
}
