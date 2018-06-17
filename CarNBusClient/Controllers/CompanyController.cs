using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CarNBusClient.Models;
using CarNBusClient.Models.CompanyViewModel;
using Microsoft.AspNetCore.Http;

namespace CarNBusClient.Controllers
{
    public class CompanyController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyController(SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }


        // GET: Company

        public async Task<IActionResult> Index(string id)
        {
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            var companies = await Utils.Get<List<Company>>("api/Company", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            string pending = "";
            string companyCreationTime = "";
            string companyName = "";
            string companyAddress = "";
            Guid pendingId = Guid.Empty;
            if (id != null && id.IndexOf("pending", StringComparison.Ordinal) != -1)
            {
                string[] tokens = id.Split('|');
                switch (tokens.Length)
                {
                    case 2:
                        id = tokens[0];
                        pending = tokens[1].Remove(0, 8);
                        pendingId = Guid.Parse(tokens[0]);
                        break;
                    case 4:
                        id = tokens[0];
                        pending = tokens[1].Remove(0, 8);
                        pendingId = Guid.Parse(tokens[0]);
                        companyName = tokens[2];
                        companyAddress = tokens[3];

                        break;
                }
            }

            var allCars = await Utils.Get<List<Car>>("api/read/car", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            foreach (var company in companies)
            {

                var cars = allCars.Where(c => c.CompanyId == company.CompanyId).ToList();
                company.Cars = cars;
            }

            var companyViewModel = new CompanyViewModel { Companies = companies };
            if (pending != "")
            {
                switch (pending)
                {
                    case "create":
                        if (companyViewModel.Companies.All(c => c.CompanyId != pendingId))
                        {
                            companyViewModel.Companies.Add(new Company(Guid.Parse(id))
                            {
                                Pending = "Create",
                                Name = companyName,
                                Address = companyAddress,
                                CreationTime = companyCreationTime
                            });
                        }

                        break;
                    case "update":
                        foreach (var company in companyViewModel.Companies)
                        {
                            if (company.CompanyId == pendingId)
                            {
                                company.Pending = "Update";
                                company.Name = companyName;
                                company.Address = companyAddress;
                                break;
                            }
                        }
                        break;
                    case "delete":
                        foreach (var company in companyViewModel.Companies)
                        {
                            if (company.CompanyId == pendingId)
                            {
                                company.Pending = "Delete";
                                break;
                            }
                        }
                        break;
                }
            }
            return View(companyViewModel);
        }

        // GET: Company/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var company = await Utils.Get<Company>("api/Company/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            return View(company);
        }

        // GET: Company/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,CreationTime")] Company company)
        {
            if (!ModelState.IsValid) return View(company);
            company.CompanyId = Guid.NewGuid();
            await Utils.Post<Company>("api/Company/", company, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            return RedirectToAction("Index", new { id = company.CompanyId + "|pending create" + "|" + company.Name + "|" + company.Address });
        }

        // GET: Company/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            var company = await Utils.Get<Company>("api/Company/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return View(company);
        }

        // POST: Company/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid companyId, [Bind("Id,CreationTime, Name, Address")] Company company)
        {
            if (!ModelState.IsValid) return View(company);
            var oldCompany = await Utils.Get<Company>("api/Company/" + companyId, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            oldCompany.Name = company.Name;
            await Utils.Put<Company>("api/Company/name/" + oldCompany.CompanyId, oldCompany, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            oldCompany.Address = company.Address;
            await Utils.Put<Company>("api/Company/address/" + oldCompany.CompanyId, oldCompany, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            return RedirectToAction("Index", new { id = oldCompany.CompanyId + "|pending update" + "|" + company.Name + "|" + company.Address });
        }

        // GET: Company/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            var company = await Utils.Get<Company>("api/Company/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await Utils.Delete<Company>("api/Company/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return RedirectToAction("Index", new { id = id + "|pending delete" });
        }

        private async Task<bool> CompanyExists(Guid id)
        {
            var companies = await Utils.Get<List<Company>>("api/Company", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return companies.Any(e => e.CompanyId == id);
        }
    }
}