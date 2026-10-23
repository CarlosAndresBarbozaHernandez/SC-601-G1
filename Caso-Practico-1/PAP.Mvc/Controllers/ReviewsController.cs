using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ReviewManager _reviewManager;

        public ReviewsController()
        {
            _reviewManager = new ReviewManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var reviews = _reviewManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.ReviewDetail =
                    _reviewManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.ReviewEdit =
                    _reviewManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.ReviewDelete =
                    _reviewManager.GetById(deleteId.Value);
            }

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;

                _reviewManager.Add(review);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                _reviewManager.Update(review);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _reviewManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}