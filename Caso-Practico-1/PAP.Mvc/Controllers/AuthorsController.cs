using PAP.Business;
using PAP.DataAccess;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly AuthorManager _authorManager;

        public AuthorsController()
        {
            _authorManager = new AuthorManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var authors = _authorManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.AuthorDetail =
                    _authorManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.AuthorEdit =
                    _authorManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.AuthorDelete =
                    _authorManager.GetById(deleteId.Value);
            }

            return View(authors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Author author)
        {
            if (ModelState.IsValid)
            {
                author.CreatedAt = DateTime.Now;

                _authorManager.Add(author);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Author author)
        {
            if (ModelState.IsValid)
            {
                _authorManager.Update(author);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _authorManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}