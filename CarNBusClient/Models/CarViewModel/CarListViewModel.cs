using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarNBusClient.Models.CarViewModel
{
    public class CarListViewModel : Car
    {
        public List<SelectListItem> CompanySelectList { get; set; }
        public List<Car> Cars { get; set; }
    }
}