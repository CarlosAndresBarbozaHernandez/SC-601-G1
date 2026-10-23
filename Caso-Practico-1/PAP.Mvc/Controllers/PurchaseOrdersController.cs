using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly PurchaseOrderManager _purchaseOrderManager;

        public PurchaseOrdersController()
        {
            _purchaseOrderManager = new PurchaseOrderManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var purchaseOrders = _purchaseOrderManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.PurchaseOrderDetail =
                    _purchaseOrderManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.PurchaseOrderEdit =
                    _purchaseOrderManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.PurchaseOrderDelete =
                    _purchaseOrderManager.GetById(deleteId.Value);
            }

            return View(purchaseOrders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                purchaseOrder.OrderDate = DateTime.Now;

                _purchaseOrderManager.Add(purchaseOrder);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                _purchaseOrderManager.Update(purchaseOrder);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _purchaseOrderManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}