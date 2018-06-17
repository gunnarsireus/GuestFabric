using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarNBusClient.Services
{
    public interface IAspNetDbLocation
    {
	    Task<string> GetAspNetDbAsync(string apiAddress);
    }
}
