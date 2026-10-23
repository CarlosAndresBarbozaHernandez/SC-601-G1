using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly SupplierManager _supplierManager;

        public SuppliersController()
        {
            _supplierManager = new SupplierManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var suppliers = _supplierManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.SupplierDetail =
                    _supplierManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.SupplierEdit =
                    _supplierManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.SupplierDelete =
                    _supplierManager.GetById(deleteId.Value);
            }

            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _supplierManager.Add(supplier);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _supplierManager.Update(supplier);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _supplierManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}