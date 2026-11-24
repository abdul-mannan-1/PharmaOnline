using Microsoft.AspNetCore.Mvc;
using PharmaOnline.Models;
using PharmaOnline.Repositories;
using System.Diagnostics;

namespace PharmaOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMedicineRepository _medicines;

        public HomeController(IMedicineRepository medicines)
        {
            _medicines = medicines;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetMedicines(string q = "")
        {
            IEnumerable<Medicine> result;
            if (string.IsNullOrWhiteSpace(q))
                result = await _medicines.GetAllAsync();
            else
                result = await _medicines.SearchAsync(q);

            return Json(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
