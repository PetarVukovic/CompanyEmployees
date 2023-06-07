using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service
{

	internal sealed class AuthenticationService : IAuthenticationService
	{
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		/*
		 * Ova se klasa koristi za pružanje API-ja za
		upravljanje korisnicima u pohrani postojanosti. Ne brine se o tome kako korisnik
		informacije se pohranjuju. Za to se oslanja na UserStore (koji u našem slučaju
		koristi Entity Framework Core).
		 */
		private readonly UserManager<User> _userManager;
		private readonly IOptions<JwtConfiguration> _configuration;
		private User? _user;
		private readonly JwtConfiguration _jwtConfiguration;


		/*
		 * We inject IOptions inside the constructor and use the Value property to 
		extract the JwtConfiguration object with all the populated properties. 
		Nothing else has to change in this class.
		 */
		public AuthenticationService( ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IOptions<JwtConfiguration> configuration )
		{
			_logger = logger;
			_mapper = mapper;
			_userManager = userManager;
			_configuration = configuration;
			_jwtConfiguration = _configuration.Value;
			
		}

		public async Task<string> CreateToken()
		{
			/*SigningCreds
			 * In ASP.NET, signing credentials are used for cryptographic signing of security tokens,
			 * such as JSON Web Tokens (JWTs). Signing credentials provide a way to verify the authenticity and integrity of the tokens, 
			 * ensuring that they haven't been tampered with.

			To generate and validate digital signatures,
			signing credentials typically consist of two components: a cryptographic key and an algorithm. 
			The key is used for both signing (when generating tokens) and verification (when validating tokens). 
			The algorithm determines the cryptographic algorithm used for signing and verification operations.
			 */
			var signingCredentials = GetSigningCredentials();
			/*Claims...
			 * 
		In the context of JWT (JSON Web Tokens) and ASP.NET, claims refer to the pieces of information contained within a JWT. 
				A claim is a statement about an entity (typically the user) and includes attributes such as the user's identity,
			roles, permissions, and any additional metadata associated with the user.

		JWTs consist of three parts: a header, a payload, and a signature. The payload contains the claims,
			which are represented as key-value pairs. Some of the commonly used standard claims in JWT include:

		"iss" (Issuer): Indicates the issuer of the token, typically the identity provider or the server generating the token.
		"sub" (Subject): Identifies the subject of the token, often representing the user or the entity the token is issued for.
		"aud" (Audience): Specifies the intended audience for the token, representing the recipient or the server to which the token is intended to be sent.
		"exp" (Expiration Time): Specifies the expiration time of the token, after which it is considered invalid.
		"nbf" (Not Before): Indicates the time before which the token is not valid.
		"iat" (Issued At): Represents the time when the token was issued.
		"jti" (JWT ID): Provides a unique identifier for the token.
			 */
			var claims = await GetClaims();

			var tokenOptions = GenerateTokenOptions( signingCredentials, claims );

			return new JwtSecurityTokenHandler().WriteToken( tokenOptions );
		}

		private JwtSecurityToken GenerateTokenOptions( SigningCredentials signingCredentials,
		List<Claim> claims )
		{
			
			var tokenOptions = new JwtSecurityToken
			(
			issuer: _jwtConfiguration.ValidIssuer,
			audience: _jwtConfiguration.ValidAudience,
			claims: claims,
			expires: DateTime.Now.AddMinutes( Convert.ToDouble(_jwtConfiguration.Expires )),
			signingCredentials: signingCredentials
			);
			return tokenOptions;
		}


		/* The GetClaims method creates a list of claims with the user 
			name inside and all the roles the user belongs to
		 */
		private async Task<List<Claim>> GetClaims()
		{
			var claims = new List<Claim>
			{
			 new Claim(ClaimTypes.Name, _user.UserName)
			};
			var roles = await _userManager.GetRolesAsync( _user );
			foreach ( var role in roles )
			{
				claims.Add( new Claim( ClaimTypes.Role, role ) );
			}
			return claims;
		}

		/* The GetSignInCredentials
		method returns our secret key as a byte array with the security 
		algorithm.
		 */
		private SigningCredentials GetSigningCredentials()
		{
			var key = Encoding.UTF8.GetBytes( Environment.GetEnvironmentVariable( "SECRET" ) );
			var secret = new SymmetricSecurityKey( key );
			return new SigningCredentials( secret, SecurityAlgorithms.Aes128CbcHmacSha256 );

		}

		/*
		 * So we map the DTO object to the User object and call the CreateAsync
		method to create that specific user in the database. The CreateAsync
		method will save the user to the database if the action succeeds or it will 
		return error messages as a result.Nakon toga, ako je korisnik kreiran, dodajemo tog korisnika imenovanim ulogama 
		— onima poslanim sa strane klijenta — i vraćamo rezultat
		 */
		public async Task<IdentityResult> RegisterUser( UserForRegistrationDto userForRegistration )
		{
			var user = _mapper.Map<User>( userForRegistration );

			var result = await _userManager.CreateAsync( user,userForRegistration.Password );

			if ( result.Succeeded  )
				await _userManager.AddToRolesAsync( user, userForRegistration.Roles );

			return result;


		}

		/*
		 * In the ValidateUser method, we fetch the user from the database and
		check whether they exist and if the password matches. The 
		UserManager<TUser> class provides the FindByNameAsync method to 
		find the user by user name and the CheckPasswordAsync to verify the 
		user’s password against the hashed password from the database. If the 
		check result is false, we log a message about failed authentication. Lastly, 
		we return the result.

		 */
		public async Task<bool> ValidateUser( UserForAuthenticationDto userForAuth )
		{
			_user = await _userManager.FindByNameAsync( userForAuth.UserName );
			var result = ( _user != null && await _userManager.CheckPasswordAsync( _user,
		   userForAuth.Password ) );
			if ( !result )
				_logger.LogWarn( $"{nameof( ValidateUser )}: Authentication failed. Wrong user name or password.");
		 return result;
		}

		public async Task<TokenDto> CreateToken( bool populateExp )
		{
			var signingCredentials=GetSigningCredentials();

			var claims = await GetClaims();

			var tokenOptions=GenerateTokenOptions(signingCredentials, claims );

			var refreshToken = GenerateRefreshToken();

			_user.RefreshToken = refreshToken;

			if(populateExp)
			{
				_user.RefreshTokenExpiryTime = DateTime.Now.AddDays( 7 );

			}
			await _userManager.UpdateAsync( _user );

			var accessToken = new JwtSecurityTokenHandler().WriteToken( tokenOptions );

			return new TokenDto( accessToken, refreshToken );
			
		}
		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using(var rng=RandomNumberGenerator.Create())
			{
				rng.GetBytes( randomNumber );
				return Convert.ToBase64String( randomNumber );
			}
		}

		/*
		 * GetPrincipalFromExpiredToken is used to get the user principal from 
			the expired access token. We make use of the ValidateToken method 
			from the JwtSecurityTokenHandler class for this purpose. This method 
			validates the token and returns the ClaimsPrincipal object.

		 */
		private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			

			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = true,
				ValidateIssuer = true,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( Environment.GetEnvironmentVariable( "SECRET" ) ) ),
				ValidateLifetime = true,
				ValidIssuer = _jwtConfiguration.ValidIssuer,
				ValidAudience =_jwtConfiguration.ValidAudience
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;

			var principal=tokenHandler.ValidateToken(token,tokenValidationParameters, out securityToken);

			var jwtSecurityToken = securityToken as JwtSecurityToken;

			if ( jwtSecurityToken == null || 
				!jwtSecurityToken.Header.Alg.Equals( SecurityAlgorithms.Aes128CbcHmacSha256, StringComparison.InvariantCultureIgnoreCase ) )
				throw new SecurityTokenException( "Invalid token" );

			return principal;
		}

		public async Task<TokenDto> RefreshToken( TokenDto tokenDto )
		{
			var principal = GetPrincipalFromExpiredToken( tokenDto.AccessToken );

			var user = await _userManager.FindByNameAsync( principal.Identity.Name );

			if ( user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now )
				throw new RefreshTokenBadRequest();

			_user = user;

			return await CreateToken( populateExp: false );
		}
	}
}
