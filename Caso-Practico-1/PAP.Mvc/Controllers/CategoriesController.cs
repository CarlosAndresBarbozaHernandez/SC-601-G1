using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoryManager _categoryManager;

        public CategoriesController()
        {
            _categoryManager = new CategoryManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var categories = _categoryManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.CategoryDetail =
                    _categoryManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.CategoryEdit =
                    _categoryManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.CategoryDelete =
                    _categoryManager.GetById(deleteId.Value);
            }

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryManager.Add(category);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryManager.Update(category);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _categoryManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}