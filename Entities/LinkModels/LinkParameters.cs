using Microsoft.AspNetCore.Http;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
	/*
	 * We are going to use this record to transfer required parameters from our 
		controller to the service layer and avoid the installation of an additional 
		NuGet package inside the Service and Service.Contracts project
	 */
	public record class LinkParameters (EmployeeParameters EmployeeParameters, HttpContext Context);
}
