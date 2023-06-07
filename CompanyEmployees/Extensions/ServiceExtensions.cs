using AspNetCoreRateLimit;
using CompanyEmployees.Presentation.Controllers;
using Contracts;
using Entities.ConfigurationModels;
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
using Microsoft.OpenApi.Models;
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
		/*
		 * One more thing. We didn’t modify anything inside the 
		ServiceExtensions/ConfigureJWT method. That’s because this 
		configuration happens during the service registration and not after 
		services are built. This means that we can’t resolve our required service 
		here. 
		 */
		public static void ConfigureJWT( this IServiceCollection services, IConfiguration
		configuration )
		{
			/*
			 * We create a new instance of the JwtConfiguration class and use the 
			Bind method that accepts the section name and the instance object as 
			parameters, to bind to the JwtSettings section directly and map 
			configuration values to respective properties inside the 
			JwtConfiguration class. Then, we just use those properties instead of 
			string keys inside square brackets, to access required values.
			 */
			var jwtSettings = new JwtConfiguration();
			configuration.Bind(jwtSettings.Section,jwtSettings);
			var secretKey = Environment.GetEnvironmentVariable( "SECRET" );
			services.AddAuthentication( opt =>
			{
				opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			} )
			.AddJwtBearer( options =>
			{

				/*
				 * Ovdje ipak treba napomenuti dvije stvari. Prvi je da imena od
				ključevi konfiguracijskih podataka i svojstva klase moraju odgovarati. Drugi je
			da ako proširite konfiguraciju, trebate proširiti i klasu,
			što može biti malo glomazno, ali bolje je od dobivanja vrijednosti upisivanjem
			stringa.
				 */
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer=jwtSettings.ValidIssuer,
					ValidAudience=jwtSettings.ValidAudience,
					IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( secretKey ) )
				};
			} );
		}

		public static void AddJwtConfigurartion(this IServiceCollection services, IConfiguration configuration )=>
			services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));


		/*
		 * We are creating two versions of SwaggerDoc because if you remember, 
		we have two versions for the Companies controller and we want to 
		separate them in our documentation.
		*/
		public static void ConfigureSwagger( this IServiceCollection services )
		{
			services.AddSwaggerGen( s =>
			{
				s.SwaggerDoc( "v1", new OpenApiInfo
				{
					Title = "Code Maze API",
					Version = "v1",
					Description = "CompanyEmployees API by CodeMaze",
					TermsOfService = new Uri( "https://example.com/terms" ),
					Contact = new OpenApiContact
					{
						Name = "John Doe",
						Email = "John.Doe@gmail.com",
						Url = new Uri( "https://twitter.com/johndoe" ),
					},
					License = new OpenApiLicense
					{
						Name = "CompanyEmployees API LICX",
						Url = new Uri( "https://example.com/license" ),
					}

				} );

				s.SwaggerDoc( "v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" } );

				var xmlFile = $"{typeof( Presentation.AssemblyReference ).Assembly.GetName().Name}.xml";
				var xmlPath = Path.Combine( AppContext.BaseDirectory, xmlFile );
				s.IncludeXmlComments( xmlPath );

				s.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "Place to add JWT with Bearer",
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				} );

				s.AddSecurityRequirement( new OpenApiSecurityRequirement()
			{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
							Name = "Bearer",
						},
						new List<string>()
					}
			} );

			} );
		}
	}
}
