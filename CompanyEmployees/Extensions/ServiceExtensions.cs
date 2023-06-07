using AspNetCoreRateLimit;
using CompanyEmployees.Presentation.Controllers;
using Contracts;
using Entities.Models;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;
using Service.Contracts;
using System.Runtime.InteropServices;
using System.Text;

namespace CompanyEmployees.Extensions
{
	public static class ServiceExtensions
	{
		public static void ConfigureCors( this IServiceCollection services ) =>
			services.AddCors( options =>
			{
				options.AddPolicy( "CorsPolicy", builder =>
				builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()
				.WithExposedHeaders( "X-Pagination" ) );
			} );

		public static void ConfigureIISIntegration( this IServiceCollection services ) =>
			services.Configure<IISOptions>( options =>
			{

			} );
		/*SINGLETON
		 * A singleton lifetime means that an object will be created once and shared throughout the lifetime of the application.
		 * This is useful for objects that are thread-safe and can be shared between multiple requests or scopes, 
		 * such as a logger or a configuration object.
		 */

		public static void ConfigureLoggerService( this IServiceCollection services ) =>
			services.AddSingleton<ILoggerManager, LoggerManager>();

		/*SCOPED
		 * This means that every time an instance of IMyService is requested within a single HTTP request, 
		 * a new instance of MyService will be created, 
		 * and that instance will be reused throughout the request. When the request ends, the instance will be disposed of.
		 */
		public static void ConfigureRepositoryManager( this IServiceCollection services ) =>
			services.AddScoped<IRepositoryManager, RepositoryManager>();


		public static void ConfigureServiceManager( this IServiceCollection services ) =>
			services.AddScoped<IServiceManager, ServiceManager>();

		public static void ConfigureSqlContext( this IServiceCollection services, IConfiguration config ) =>
			services.AddDbContext<RepositoryContext>( opt =>
			{
				opt.UseSqlServer( config.GetConnectionString( "sqlConnection" ) );
			} );

		/*
				 * With the AddApiVersioning method, we are adding service API 
		versioning to the service collection. We are also using a couple of 
		properties to initially configure versioning:
		• ReportApiVersions adds the API version to the response header.
		• AssumeDefaultVersionWhenUnspecified does exactly that. It 
		specifies the default API version if the client doesn’t send one.
		• DefaultApiVersion sets the default version count.
		 */
		public static void ConfigureVersioning( this IServiceCollection services ) =>
			services.AddApiVersioning( config =>
			{
				config.ReportApiVersions = true;
				config.AssumeDefaultVersionWhenUnspecified = true;
				config.DefaultApiVersion = new ApiVersion( 1, 0 );
				config.ApiVersionReader = new HeaderApiVersionReader( "api-version" );
				config.Conventions.Controller<CompaniesController>().HasApiVersion( new ApiVersion( 1, 0 ) );
				config.Conventions.Controller<CompaniesV2Controller>().HasDeprecatedApiVersion( new ApiVersion( 2, 0 ) );
			} );

		public static IMvcBuilder AddCustomCSVFormatter( this IMvcBuilder builder ) =>
		 builder.AddMvcOptions( config => config.OutputFormatters.Add( new
		CsvOutputFormatter() ) );


		/*
		 * We are registering two new custom media types for the JSON and XML 
		output formatters. This ensures we don’t get a 406 Not Acceptable 
		response.

		 */
		public static void AddCustomMediaTypes( this IServiceCollection services )
		{
			services.Configure<MvcOptions>( config =>
			{
				var systemTextJsonOutputFormatter = config.OutputFormatters
				.OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();

				if ( systemTextJsonOutputFormatter != null )
				{
					systemTextJsonOutputFormatter.SupportedMediaTypes.Add( "application/vnd.codemaze.hateoas+json" );
					systemTextJsonOutputFormatter.SupportedMediaTypes.Add( "application/vnd.codemaze.apiroot+json" );
				}
				var xmlOutputFormatter = config.OutputFormatters
				.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();


				if ( xmlOutputFormatter != null )
				{
					xmlOutputFormatter.SupportedMediaTypes
					.Add( "application/vnd.codemaze.hateoas+xml" );
					xmlOutputFormatter.SupportedMediaTypes
					.Add( "application/vnd.codemaze.apiroot+xml" );

				}

			} );
		}

		public static void ConfigureResponseCaching( this IServiceCollection services ) =>
			services.AddResponseCaching();

		public static void ConfigureHttpCacheHeaders( IServiceCollection services ) =>
			services.AddHttpCacheHeaders(
				( expirationOpt ) =>
				{
					expirationOpt.MaxAge = 65;
					expirationOpt.CacheLocation = CacheLocation.Private;
				},
				( validationOpt )=>
				{
				validationOpt.MustRevalidate = true;

				});

		public static void ConfigureRateLimitingOptions(this IServiceCollection services)
		{
			/*
			 * We create a rate limit rules first, for now just one, stating that three
				requests are allowed in a five-minute period for any endpoint in our API. 
				Then, we configure IpRateLimitOptions to add the created rule. Finally, we 
				have to register rate limit stores, configuration, and processing strategy
				as a singleton. They serve the purpose of storing rate limit counters and 
				policies as well as adding configuration.
			 */
			var rateLimitRules = new List<RateLimitRule>
			{
				new RateLimitRule
				{
				Endpoint = "*",
				Limit = 30,
				Period = "5m"
				}

			};
			services.Configure<IpRateLimitOptions>( opt => {
				opt.GeneralRules =rateLimitRules;
			} );
			services.AddSingleton<IRateLimitCounterStore,MemoryCacheRateLimitCounterStore>();
			services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
		}

		public static void ConfigureIdentity(this IServiceCollection services)
		{
			var builder = services.AddIdentity<User, IdentityRole>( o =>
			{

				o.Password.RequireDigit = true;
				o.Password.RequireLowercase = false;
				o.Password.RequireUppercase = false;
				o.Password.RequireNonAlphanumeric = false;
				o.Password.RequiredLength = 10;
				o.User.RequireUniqueEmail = true;

			} )
			.AddEntityFrameworkStores<RepositoryContext>()
			.AddDefaultTokenProviders();
				
				
		}
		public static void ConfigureJWT( this IServiceCollection services, IConfiguration
		configuration )
		{
			var jwtSettings = configuration.GetSection( "JwtSettings" );
			var secretKey = Environment.GetEnvironmentVariable( "SECRET" );
			services.AddAuthentication( opt =>
			{
				opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			} )
			.AddJwtBearer( options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings["validIssuer"],
					ValidAudience = jwtSettings["validAudience"],
					IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( secretKey ) )
				};
			} );
		}

	}
}
