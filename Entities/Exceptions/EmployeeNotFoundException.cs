﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
	public sealed class EmployeeNotFoundException:NotFoundException
	{
		public EmployeeNotFoundException(Guid Id)
			:base($"Employee with id {Id} doesnt exist in database")
		{

		}
	}
}
