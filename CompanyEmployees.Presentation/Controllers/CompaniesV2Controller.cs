using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{

	//This code is an attribute routing for an API controller. The Route attribute specifies the URL for the controller, in this case "api/companies". The ApiController
	// attribute indicates that the controller is an API controller, which means it will handle HTTP requests.
	[Route( "api/companies" )]
	[ApiController]
	[ApiExplorerSettings(GroupName ="v2")]
	public class CompaniesV2Controller : ControllerBase
	{
		private readonly IServiceManager _service;

		public CompaniesV2Controller( IServiceManager service )
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetCompanies()
		{
			var companies = await _service.CompanyService.GetAllCompaniesAsync( trackChanges: false );

			/*
			 * We are creating a projection from our companies collection by iterating 
			through each element, modifying the Name property to contain the V2
			suffix, and extracting it to a new collection companiesV2.
			 */
			var companies2 = companies.Select( x => $"{x.Name} V2" );

			return Ok( companies2 );
		}


	}
}
