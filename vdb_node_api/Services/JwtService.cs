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
using System.Security.Cryptography;
using IdentityModel;

namespace vdb_node_api.Services
{

	/* Singleton-сервис, заниющйся генерацией Jwt по набору утверждений
	 * с дальнейшей генерацией HMAC SHA512 подписи токена.
	 */
	public sealed class JwtService
	{
		private readonly SettingsProviderService _settingsProvider;
		private readonly JwtSecurityTokenHandler _tokenHandler;
		private readonly SymmetricSecurityKey _symmetricKey;
		private readonly SigningCredentials _signingCredentials;
		private JwtServiceSettings _settings => _settingsProvider.JwtServiceSettings;

		public JwtService(SettingsProviderService settingsProvider)
		{
			this._settingsProvider = settingsProvider;

			this._tokenHandler = new JwtSecurityTokenHandler();
			this._symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.SigningKey));
			this._signingCredentials = new SigningCredentials(this._symmetricKey, SecurityAlgorithms.HmacSha512Signature);
		}



		public string GenerateAccessToken(ApplicationAccount account)
		{
			return GenerateJwtToken(new Claim[] {
				new Claim(nameof(account.Id), account.Id.ToString()),
				new Claim(nameof(account.LastAccessedUtc),account.LastAccessedUtc.Ticks.ToString())
			}, DateTime.UtcNow.AddDays(_settings.AccessLifespanDays));
		}

		public const string RefreshKeyFieldName = "refreshkey";
		public const string RefreshTokenCookieName = "X-Refresh-Token";
		public string GenerateRefreshToken(ApplicationAccount account, 
			out string refreshKey, out string refreshKeyHash)
		{
			var key = RandomNumberGenerator.GetBytes(64);
			refreshKey = Base64Url.Encode(key);
			refreshKeyHash = Base64Url.Encode(SHA512.HashData(key));

			return GenerateJwtToken(new Claim[]
			{
				new Claim(nameof(account.Id), account.Id.ToString()),
				new Claim(nameof(account.LastAccessedUtc),account.LastAccessedUtc.Ticks.ToString()),
				new Claim(RefreshKeyFieldName,refreshKey)
			}, DateTime.UtcNow.AddDays(_settings.RefreshLifespanDays));
		}

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

		//[Obsolete(@"Use built-in 'Authorize' attr.")]
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
