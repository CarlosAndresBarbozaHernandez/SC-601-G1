using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerManager _customerManager;

        public CustomersController()
        {
            _customerManager = new CustomerManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var customers = _customerManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.CustomerDetail =
                    _customerManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.CustomerEdit =
                    _customerManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.CustomerDelete =
                    _customerManager.GetById(deleteId.Value);
            }

            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                customer.UpdatedAt = DateTime.Now;

                _customerManager.Add(customer);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.UpdatedAt = DateTime.Now;

                _customerManager.Update(customer);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _customerManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}