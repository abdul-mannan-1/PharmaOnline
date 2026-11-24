using Microsoft.AspNetCore.Mvc;
using PharmaOnline.Repositories;

namespace PharmaOnline.Controllers
{
    public class MedicineController : Controller
    {
        private readonly IMedicineRepository _medicines;

        public MedicineController(IMedicineRepository medicines)
        {
            _medicines = medicines;
        }

        public IActionResult Details(int id)
        {
            ViewBag.MedicineId = id;
            return View();
        }

        public async Task<IActionResult> GetDetails(int id)
        {
            var medicine = await _medicines.GetByIdAsync(id);
            if (medicine == null) return NotFound();
            return Json(medicine);
        }
    }
}
