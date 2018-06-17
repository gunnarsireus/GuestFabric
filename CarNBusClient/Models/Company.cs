using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CarNBusClient.Models
{
	public class Company
	{
		public Company()
		{
			CreationTime = DateTime.Now.ToString(new CultureInfo("sv-SE"));
		}
		public Company(Guid companyId) : this()
		{
			CompanyId = companyId;
		}
		public Guid CompanyId { get; set; }

		[Display(Name = "Created date")]
		public string CreationTime { get; set; }
		[Display(Name = "Name")]
		public string Name { get; set; }

		[Display(Name = "Address")]
		public string Address { get; set; }

		public ICollection<Car> Cars { get; set; }
		public string Pending { get; set; }  //Pending change in database
	}
}
