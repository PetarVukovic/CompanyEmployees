using Contracts;
using Entities.ErrorModels;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace CompanyEmployees.Extensions
{
	public static class ExceptionMiddlewareExtensions
	{
		public static void ConfigureExceptionHandler(this WebApplication app,ILoggerManager logger)
		{
			/*
			 In the code above, we create an extension method, on top of the 
			WebApplication type, and we call the UseExceptionHandler method.
			That method adds a middleware to the pipeline that will catch exceptions, 
			log them, and re-execute the request in an alternate pipeline.
			 */
			app.UseExceptionHandler( appError =>
			{
				appError.Run( async context =>
				{
					context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
					context.Response.ContentType = "application/json";

					var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
					/*
					 * U ovom specifičnom isječku koda, naredba switch provjerava vrstu pogreške
					 * koja se dogodila i prema tome postavlja statusni kod odgovora.
					 * Prvi slučaj provjerava je li vrsta pogreške NotFoundException 
					 * i postavlja statusni kod na 404 ako jest.
					 * Drugi slučaj koristi zamjenski znak _ za podudaranje bilo koje druge vrste pogreške 
					 * i postavlja statusni kod na 500 u tom slučaju.
					 */
					if ( contextFeature != null)
					{
						context.Response.StatusCode = contextFeature.Error switch
						{
							NotFoundException => StatusCodes.Status404NotFound,
							BadRequestException => StatusCodes.Status400BadRequest,

							_ => StatusCodes.Status500InternalServerError
						};
						logger.LogError( $"Something went wrong:{contextFeature.Error}" );
						await context.Response.WriteAsync( new ErrorDetails()
						{
							StatusCode = context.Response.StatusCode,
							Message = contextFeature.Error.Message,

						}.ToString() );
					
					}
				} );
			} );
		}

		
	}
}
