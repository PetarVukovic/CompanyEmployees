using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Contracts;


namespace CompanyEmployees.Extensions
{
	public static class ServiceExtensions
	{
		public static void ConfigureCors(this IServiceCollection services) =>
			services.AddCors( options =>
			{
				options.AddPolicy( "CorsPolicy", builder =>
				builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()
				.WithExposedHeaders( "X-Pagination" ) );
			});

		public static void ConfigureIISIntegration(this IServiceCollection services) =>
			services.Configure<IISOptions>( options =>
			{

			} );
		/*SINGLETON
		 * A singleton lifetime means that an object will be created once and shared throughout the lifetime of the application.
		 * This is useful for objects that are thread-safe and can be shared between multiple requests or scopes, 
		 * such as a logger or a configuration object.
		 */

		public static void ConfigureLoggerService(this IServiceCollection services)=>
			services.AddSingleton<ILoggerManager,LoggerManager>();

		/*SCOPED
		 * This means that every time an instance of IMyService is requested within a single HTTP request, 
		 * a new instance of MyService will be created, 
		 * and that instance will be reused throughout the request. When the request ends, the instance will be disposed of.
		 */
		public static void ConfigureRepositoryManager(this IServiceCollection services) =>
			services.AddScoped<IRepositoryManager, RepositoryManager>();


		public static void ConfigureServiceManager(this IServiceCollection services) =>
			services.AddScoped<IServiceManager,ServiceManager>();

		public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration config) =>
			services.AddDbContext<RepositoryContext>( opt =>
			{
				opt.UseSqlServer( config.GetConnectionString( "sqlConnection" ) );
			} );

		public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
		 builder.AddMvcOptions( config => config.OutputFormatters.Add( new
		CsvOutputFormatter() ) );


		/*
		 * We are registering two new custom media types for the JSON and XML 
		output formatters. This ensures we don’t get a 406 Not Acceptable 
		response.

		 */
		public static void AddCustomMediaTypes (this IServiceCollection services)
		{
			services.Configure<MvcOptions>( config =>
			{
				var systemTextJsonOutputFormatter=config.OutputFormatters
				.OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();

				if(systemTextJsonOutputFormatter !=null )
				{
					systemTextJsonOutputFormatter.SupportedMediaTypes.Add( "application/vnd.codemaze.hateoas+json" );
				}
				var xmlOutputFormatter = config.OutputFormatters
				.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();


				if ( xmlOutputFormatter != null )
				{
					xmlOutputFormatter.SupportedMediaTypes
					.Add( "application/vnd.codemaze.hateoas+xml" );
				}

			} );
		}

	}
}
