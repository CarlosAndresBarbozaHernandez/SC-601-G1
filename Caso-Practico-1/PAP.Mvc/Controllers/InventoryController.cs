using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryManager _inventoryManager;

        public InventoryController()
        {
            _inventoryManager = new InventoryManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var inventory = _inventoryManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.InventoryDetail =
                    _inventoryManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.InventoryEdit =
                    _inventoryManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.InventoryDelete =
                    _inventoryManager.GetById(deleteId.Value);
            }

            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.LastRestockedAt = DateTime.Now;

                _inventoryManager.Add(inventory);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.LastRestockedAt = DateTime.Now;

                _inventoryManager.Update(inventory);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _inventoryManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}