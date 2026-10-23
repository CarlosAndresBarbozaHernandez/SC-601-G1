using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class AddressesController : Controller
    {
        private readonly AddressManager _addressManager;

        public AddressesController()
        {
            _addressManager = new AddressManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var addresses = _addressManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.AddressDetail =
                    _addressManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.AddressEdit =
                    _addressManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.AddressDelete =
                    _addressManager.GetById(deleteId.Value);
            }

            return View(addresses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Address address)
        {
            if (ModelState.IsValid)
            {
                _addressManager.Add(address);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                _addressManager.Update(address);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _addressManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}