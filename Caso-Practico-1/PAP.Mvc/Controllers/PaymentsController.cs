using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PaymentManager _paymentManager;

        public PaymentsController()
        {
            _paymentManager = new PaymentManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var payments = _paymentManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.PaymentDetail =
                    _paymentManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.PaymentEdit =
                    _paymentManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.PaymentDelete =
                    _paymentManager.GetById(deleteId.Value);
            }

            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentDate = DateTime.Now;

                _paymentManager.Add(payment);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _paymentManager.Update(payment);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _paymentManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}