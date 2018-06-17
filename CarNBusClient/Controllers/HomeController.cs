using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CarNBusClient.Models;
using CarNBusClient.Models.HomeViewModel;
using Microsoft.AspNetCore.Http;

namespace CarNBusClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.ApiAddress = _httpContextAccessor.HttpContext.Session.GetString("ApiAddress");

            List<Company> companies;
            try
            {
                companies = await Utils.Get<List<Company>>("api/Company", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            }
            catch (Exception e)
            {
                TempData["CustomError"] = "No contact with server! CarNBusAPI must be started before CarNBusClient could start!";
                return View(new HomeViewModel { Companies = new List<Company>() });
            }

            var allCars = await Utils.Get<List<Car>>("api/read/Car", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            foreach (var company in companies)
            {
                var companyCars = allCars.Where(o => o.CompanyId == company.CompanyId).ToList();
                company.Cars = companyCars;
            }
            var homeViewModel = new HomeViewModel { Companies = companies };
            return View(homeViewModel);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}