using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class PurchaseOrderItemsController : Controller
    {
        private readonly PurchaseOrderItemManager _purchaseOrderItemManager;

        public PurchaseOrderItemsController()
        {
            _purchaseOrderItemManager = new PurchaseOrderItemManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var purchaseOrderItems = _purchaseOrderItemManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.PurchaseOrderItemDetail =
                    _purchaseOrderItemManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.PurchaseOrderItemEdit =
                    _purchaseOrderItemManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.PurchaseOrderItemDelete =
                    _purchaseOrderItemManager.GetById(deleteId.Value);
            }

            return View(purchaseOrderItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {
                _purchaseOrderItemManager.Add(purchaseOrderItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {
                _purchaseOrderItemManager.Update(purchaseOrderItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _purchaseOrderItemManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}