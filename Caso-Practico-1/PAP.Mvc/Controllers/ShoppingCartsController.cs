using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class ShoppingCartsController : Controller
    {
        private readonly ShoppingCartManager _shoppingCartManager;

        public ShoppingCartsController()
        {
            _shoppingCartManager = new ShoppingCartManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var shoppingCarts = _shoppingCartManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.ShoppingCartDetail =
                    _shoppingCartManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.ShoppingCartEdit =
                    _shoppingCartManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.ShoppingCartDelete =
                    _shoppingCartManager.GetById(deleteId.Value);
            }

            return View(shoppingCarts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                shoppingCart.CreatedAt = DateTime.Now;
                shoppingCart.UpdatedAt = DateTime.Now;

                _shoppingCartManager.Add(shoppingCart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                shoppingCart.UpdatedAt = DateTime.Now;

                _shoppingCartManager.Update(shoppingCart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _shoppingCartManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}