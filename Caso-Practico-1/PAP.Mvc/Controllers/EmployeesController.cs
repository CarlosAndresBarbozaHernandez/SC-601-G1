using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeManager _employeeManager;

        public EmployeesController()
        {
            _employeeManager = new EmployeeManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var employees = _employeeManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.EmployeeDetail =
                    _employeeManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.EmployeeEdit =
                    _employeeManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.EmployeeDelete =
                    _employeeManager.GetById(deleteId.Value);
            }

            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.CreatedAt = DateTime.Now;

                _employeeManager.Add(employee);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _employeeManager.Update(employee);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _employeeManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}