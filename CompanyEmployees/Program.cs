using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using Microsoft.AspNetCore.Mvc.Formatters;
using Service.DataShaping;
using Shared.DataTransferObjects;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Utility;

var builder = WebApplication.CreateBuilder( args );

LogManager.LoadConfiguration( string.Concat( Directory.GetCurrentDirectory(), "/nlog.config" ) );


builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext( builder.Configuration );
builder.Services.AddScoped<IDataShaper<EmployeeDto>,DataShaper<EmployeeDto>>();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();



builder.Services.Configure<ApiBehaviorOptions>( options =>
{
	options.SuppressModelStateInvalidFilter = true;
} );

/*
  But now, our app will find all of the controllers
inside of the Presentation project and configure them with the
framework. They are going to be treated the same as if they were defined
conventionally.

We added the ReturnHttpNotAcceptable = true option, which tells 
the server that if the client tries to negotiate for the media type the 
server doesn’t support, it should return the 406 Not Acceptable status 
code.
 * */
builder.Services.AddControllers( config =>
{
	config.RespectBrowserAcceptHeader = true;
	config.ReturnHttpNotAcceptable = true;
	config.InputFormatters.Insert(0,GetJsonPatchInputFormatter());

} ).AddXmlDataContractSerializerFormatters()
.AddCustomCSVFormatter()
.AddApplicationPart( typeof( CompanyEmployees.Presentation.AssemblyReference ).Assembly );
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCustomMediaTypes();


var app = builder.Build();

var logger=app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler( logger );

if(app.Environment.IsProduction())
	app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders( new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors( "CorsPolicy" );

app.UseAuthorization();

/*
routes are configured with the
app.MapControllers method, which adds endpoints for controller
actions without specifying any routes.
 */
app.MapControllers();

app.Run();

NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson()
.Services.BuildServiceProvider()
.GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
.OfType<NewtonsoftJsonPatchInputFormatter>().First();