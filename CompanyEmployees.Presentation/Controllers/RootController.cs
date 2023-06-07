using Entities.LinkModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
	[Route("api")]
	[ApiController]
	public class RootController:ControllerBase
	{
		private readonly LinkGenerator _linkGenerator;

		public RootController(LinkGenerator linkGenerator)
		{
			_linkGenerator = linkGenerator;
		}

		/*
		 * There are several links that we are going to create in this action. The link 
		to the document itself and links to actions available on the URIs at the 
		root level (actions from the Companies controller). We are not creating 
		links to employees, because they are children of the company — and in 
		our API if we want to fetch employees, we have to fetch the company 
		first. If we inspect our CompaniesController, we can see that GetCompanies
		and CreateCompany are the only actions on the root URI level 
		(api/companies). Therefore, we are going to create links only to them.

		 */

		[HttpGet(Name ="GetRoot")]
		public IActionResult GetRoot( [FromHeader( Name = "Accept" )] string mediaType )
		{
			/*
			 * In this action, we generate links only if a custom media type is provided
			from the Accept header. Otherwise, we return NoContent(). To generate 
			links, we use the GetUriByName method from the LinkGenerator class.

			 */
			if ( mediaType.Contains( "application/vnd.codemaze.apiroot" ) )
			{
				var links = new List<Link>
				{
					new Link
					{
						Href=_linkGenerator.GetUriByName(HttpContext,nameof(GetRoot),new{}),
						Method= "GET",
						Rel="self"
					},
					new Link
					{
						Href=_linkGenerator.GetUriByName(HttpContext,"GetCompanies",new{}),
						Rel="companies",
						Method= "GET",

					},
					new Link
					{
						Href=_linkGenerator.GetUriByName(HttpContext,"CreateCompany",new{}),
						Rel="create_company",
						Method="POST",
					}
				};
				return Ok(links);
			}
			return NoContent();
		}

	}
}
