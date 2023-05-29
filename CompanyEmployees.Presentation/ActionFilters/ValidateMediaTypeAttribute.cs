using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.ActionFilters
{
	public class ValidateMediaTypeAttribute : IActionFilter
	{
		public void OnActionExecuted(ActionExecutedContext context) { }


		/*
		 * We check for the existence of the Accept header first. If it’s not present,
		we return BadRequest. If it is, we parse the media type — and if there is 
		no valid media type present, we return BadRequest.

		 */
		public void OnActionExecuting(ActionExecutingContext context)
		{
			var acceptHeaderPresent=context.HttpContext.Request.Headers.ContainsKey("Accept");

			if(!acceptHeaderPresent)
			{
				context.Result = new BadRequestObjectResult( $"Accept header is missing" );
				return;
			}

			var mediaType = context.HttpContext.Request.Headers["Accept"].FirstOrDefault();

			if(!MediaTypeHeaderValue.TryParse(mediaType,out MediaTypeHeaderValue? outMediatype))
			{
				context.Result = new BadRequestObjectResult( $"Media type not present.Please add accept header with the required media type" );
				return;
			}
			context.HttpContext.Items.Add( "AcceptHeaderMediaType", outMediatype );
		}
	}
}
