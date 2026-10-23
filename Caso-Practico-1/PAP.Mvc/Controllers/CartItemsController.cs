using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class CartItemsController : Controller
    {
        private readonly CartItemManager _cartItemManager;

        public CartItemsController()
        {
            _cartItemManager = new CartItemManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var cartItems = _cartItemManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.CartItemDetail =
                    _cartItemManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.CartItemEdit =
                    _cartItemManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.CartItemDelete =
                    _cartItemManager.GetById(deleteId.Value);
            }

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                cartItem.AddedAt = DateTime.Now;
                _cartItemManager.Add(cartItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                _cartItemManager.Update(cartItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _cartItemManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}