using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class PublishersController : Controller
    {
        private readonly PublisherManager _publisherManager;

        public PublishersController()
        {
            _publisherManager = new PublisherManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var publishers = _publisherManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.PublisherDetail =
                    _publisherManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.PublisherEdit =
                    _publisherManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.PublisherDelete =
                    _publisherManager.GetById(deleteId.Value);
            }

            return View(publishers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                publisher.CreatedAt = DateTime.Now;

                _publisherManager.Add(publisher);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                _publisherManager.Update(publisher);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _publisherManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}