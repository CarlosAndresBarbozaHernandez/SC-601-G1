using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly OrderItemManager _orderItemManager;

        public OrderItemsController()
        {
            _orderItemManager = new OrderItemManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var orderItems = _orderItemManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.OrderItemDetail =
                    _orderItemManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.OrderItemEdit =
                    _orderItemManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.OrderItemDelete =
                    _orderItemManager.GetById(deleteId.Value);
            }

            return View(orderItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _orderItemManager.Add(orderItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _orderItemManager.Update(orderItem);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _orderItemManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}