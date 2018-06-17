using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarNBusClient.Models.HomeViewModel
{
    public class HomeViewModel : Car
    {
	    public List<Company> Companies { get; set; }
	}
}