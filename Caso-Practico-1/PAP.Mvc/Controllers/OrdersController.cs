using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderManager _orderManager;

        public OrdersController()
        {
            _orderManager = new OrderManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var orders = _orderManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.OrderDetail =
                    _orderManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.OrderEdit =
                    _orderManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.OrderDelete =
                    _orderManager.GetById(deleteId.Value);
            }

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                _orderManager.Add(order);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                order.UpdatedAt = DateTime.Now;

                _orderManager.Update(order);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _orderManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}