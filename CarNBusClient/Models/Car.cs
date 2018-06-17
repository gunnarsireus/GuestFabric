using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace CarNBusClient.Models
{
	public class Car
	{
		public Car()
		{
			CreationTime = DateTime.Now.ToString(new CultureInfo("sv-SE"));
			Online = true;
            Speed = 550;
		}
		public Car(Guid companyId) : this()
		{
			CompanyId = companyId;
		}
		public Guid CarId { get; set; }
		public Guid CompanyId { get; set; }

		[Display(Name = "Created date")]
		public string CreationTime { get; set; }

		[Display(Name = "VIN (VehicleID)")]
		[RegularExpression(@"^[A-Z0-9]{6}\d{11}$", ErrorMessage = "{0} denoted as X1Y2Z300001239876")]
		[Remote("VinAvailable", "Car", ErrorMessage = "VIN upptaget")]
		public string VIN { get; set; }

		[Display(Name = "Reg. Nbr.")]
		[RegularExpression(@"^[A-Z]{3}\d{3}$", ErrorMessage = "{0} denoted as XYZ123")]
		[Remote("RegNrAvailable","Car", ErrorMessage = "Registration number taken")]
		public string RegNr { get; set; }

		[Display(Name = "Status")]
		public bool Online { get; set; }

		[Display(Name = "Online (X) or Offline ()?")]
		public string OnlineOrOffline => (this.Online)?"Online":"Offline";

		public bool Locked { get; set; } //Used to block changes of Online/Offline status
		public string Pending { get; set; }  //Pending change sin database
        public int Speed { get; set; }
        [Display(Name = "Km/h")]
        public string ConvertSpeed => (Speed / 10).ToString() + "," + (Speed % 10).ToString();
        public bool OldOnline { get; set; }
        public long OldSpeed { get; set; }
	}
}
