
using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class GenresController : Controller
    {
        private readonly GenreManager _genreManager;

        public GenresController()
        {
            _genreManager = new GenreManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var genres = _genreManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.GenreDetail =
                    _genreManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.GenreEdit =
                    _genreManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.GenreDelete =
                    _genreManager.GetById(deleteId.Value);
            }

            return View(genres);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _genreManager.Add(genre);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _genreManager.Update(genre);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _genreManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}