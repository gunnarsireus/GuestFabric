using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarNBusClient.Models;
using CarNBusClient.Models.CarViewModel;
using Microsoft.AspNetCore.Http;

namespace CarNBusClient.Controllers
{
    public class CarController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CarController(SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Car
        public async Task<IActionResult> Index(string id)
        {
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            var companies = await Utils.Get<List<Company>>("api/Company", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            string pending = "";
            string carRegNr = "";
            string carVIN = "";
            bool carOnline = false;
            string carCreationTime = ";";
            int carSpeed = 0;
            Guid pendingId = Guid.Empty;
            if (id != null && id.IndexOf("pending", StringComparison.Ordinal) != -1)
            {
                string[] tokens = id.Split(',');
                switch (tokens.Length)
                {
                    case 3:
                        id = tokens[0];
                        pending = tokens[1].Remove(0, 8);
                        pendingId = Guid.Parse(tokens[2]);
                        break;
                    case 8:
                        id = tokens[0];
                        pending = tokens[1].Remove(0, 8);
                        pendingId = Guid.Parse(tokens[2]);
                        carRegNr = tokens[3];
                        carVIN = tokens[4];
                        carOnline = bool.Parse(tokens[5]);
                        carCreationTime = tokens[6];
                        carSpeed = int.Parse(tokens[7]);
                        break;
                }
            }

            if (companies.Any() && id == null)
                id = companies[0].CompanyId.ToString();
            var selectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select company",
                    Value = ""
                }
            };
            selectList.AddRange(companies.Select(company => new SelectListItem
            {
                Text = company.Name,
                Value = company.CompanyId.ToString(),
                Selected = company.CompanyId.ToString() == id
            }));
            var cars = new List<Car>();

            if (id != null)
            {
                cars = await Utils.Get<List<Car>>("api/read/Car", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
                var companyId = new Guid(id);
                cars = cars.Where(o => o.CompanyId == companyId).ToList();
            }

            var carListViewModel = new CarListViewModel()
            {
                CompanySelectList = selectList,
                Cars = cars
            };
            if (pending != "")
            {
                switch (pending)
                {
                    case "create":
                        if (carListViewModel.Cars.All(c => c.CarId != pendingId))
                        {
                            carListViewModel.Cars.Add(new Car(Guid.Parse(id))
                            {
                                Pending = "Create",
                                CarId = pendingId,
                                RegNr = carRegNr,
                                VIN = carVIN,
                                CreationTime = carCreationTime,
                                Online = carOnline,
                                Speed = carSpeed
                            });
                        }

                        break;
                    case "update":
                        foreach (var car in carListViewModel.Cars)
                        {
                            if (car.CarId == pendingId)
                            {
                                car.Pending = "Update";
                                car.Online = carOnline;
                                car.Speed = carSpeed;
                                break;
                            }
                        }
                        break;
                    case "delete":
                        foreach (var car in carListViewModel.Cars)
                        {
                            if (car.CarId == pendingId)
                            {
                                car.Pending = "Delete";
                                break;
                            }
                        }
                        break;
                    case "edit":
                        foreach (var car in carListViewModel.Cars)
                        {
                            if (car.CarId == pendingId)
                            {
                                car.Pending = "Edit";
                                break;
                            }
                        }
                        break;
                    case "timeout":
                        foreach (var car in carListViewModel.Cars)
                        {
                            if (car.CarId == pendingId)
                            {
                                car.Pending = "Timeout";
                                break;
                            }
                        }
                        break;
                }
            }

            ViewBag.CompanyId = id;
            return View(carListViewModel);
        }

        // GET: Car/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var car = await Utils.Get<Car>("api/read/Car/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            var company = await Utils.Get<Company>("api/Company/" + car.CompanyId, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            ViewBag.CompanyName = company.Name;
            return View(car);
        }

        // GET: Car/Create
        public async Task<IActionResult> Create(string id)
        {
            var companyId = new Guid(id);
            var car = new Car
            {
                CompanyId = companyId,
            };
            var company = await Utils.Get<Company>("api/Company/" + companyId, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            ViewBag.CompanyName = company.Name;
            return View(car);
        }

        // POST: Car/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyId,VIN,RegNr,Online,Speed")] Car car)
        {
            if (!ModelState.IsValid) return View(car);
            car.CarId = Guid.NewGuid();
            await Utils.Post<Car>("api/write/Car/", car, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));

            return RedirectToAction("Index", new { id = car.CompanyId + ",pending create," + car.CarId + "," + car.RegNr + "," + car.VIN + "," + car.Online + "," + car.CreationTime + "," + car.Speed });
        }

        // GET: Car/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var car = await Utils.Get<Car>("api/read/Car/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            if (car.Locked)
            {
                return RedirectToAction("Index", new { id = car.CompanyId + ",pending edit," + car.CarId + "," + car.RegNr + "," + car.VIN + "," + car.Online + "," + car.CreationTime + "," + car.Speed });
            }
            car.Locked = true; //Prevent updates of Online/Offline while editing
            await Utils.Put<Car>("api/write/car/locked/" + id, car, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            var company = await Utils.Get<Company>("api/Company/" + car.CompanyId, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            ViewBag.CompanyName = company.Name;
            car.OldOnline = car.Online;
            car.OldSpeed = car.Speed;
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid carid, [Bind("CarId, Online, Speed, OldOnline, OldSpeed")] Car car)
        {
            if (!ModelState.IsValid) return View(car);
            var oldCar = await Utils.Get<Car>("api/read/Car/" + car.CarId, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            if (!oldCar.Locked || oldCar.Speed != car.OldSpeed || oldCar.Online != car.OldOnline)
            {
                return RedirectToAction("Index", new { id = oldCar.CompanyId + ",pending timeout," + oldCar.CarId + "," + oldCar.RegNr + "," + oldCar.VIN + "," + car.Online + "," + oldCar.CreationTime + "," + car.Speed });
            }
            oldCar.Online = car.Online;
            await Utils.Put<Car>("api/write/Car/online/" + oldCar.CarId, oldCar, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            oldCar.Speed = car.Speed;
            await Utils.Put<Car>("api/write/Car/speed/" + oldCar.CarId, oldCar, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            oldCar.Locked = false; //Enable updates of Online/Offline when editing done
            await Utils.Put<Car>("api/write/Car/locked/" + oldCar.CarId, oldCar, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return RedirectToAction("Index", new { id = oldCar.CompanyId + ",pending update," + car.CarId + "," + car.RegNr + "," + car.VIN + "," + car.Online + "," + car.CreationTime + "," + car.Speed });
        }

        // GET: Car/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var car = await Utils.Get<Car>("api/read/Car/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            if (car.Locked)
            {
                return RedirectToAction("Index", new { id = car.CompanyId + ",pending edit," + car.CarId + "," + car.RegNr + "," + car.VIN + "," + car.Online + "," + car.CreationTime + "," + car.Speed });
            }
            car.Locked = true; //Prevent updates of Online/Offline while editing
            await Utils.Put<Car>("api/write/car/locked/" + id, car, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var oldCar = await Utils.Get<Car>("api/read/Car/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            if (!oldCar.Locked)
            {
                return RedirectToAction("Index", new { id = oldCar.CompanyId + ",pending timeout," + oldCar.CarId + "," + oldCar.RegNr + "," + oldCar.VIN + "," + oldCar.Online + "," + oldCar.CreationTime + "," + oldCar.Speed });
            }
            await Utils.Delete<Car>("api/write/Car/" + id, _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return RedirectToAction("Index", new { id = oldCar.CompanyId + ",pending delete," + id });
        }

        public async Task<bool> RegNrAvailable(string regNr)
        {
            var cars = await Utils.Get<List<Car>>("api/read/Car", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return cars.All(c => c.RegNr != regNr);
        }

        public async Task<bool> VinAvailable(string vin)
        {
            var cars = await Utils.Get<List<Car>>("api/read/Car", _httpContextAccessor.HttpContext.Session.GetString("ApiAddress"));
            return cars.All(c => c.VIN != vin);
        }
    }
}