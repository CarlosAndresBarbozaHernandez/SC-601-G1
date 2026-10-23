using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager _roleManager;

        public RolesController()
        {
            _roleManager = new RoleManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            var roles = _roleManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.RoleDetail =
                    _roleManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.RoleEdit =
                    _roleManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.RoleDelete =
                    _roleManager.GetById(deleteId.Value);
            }

            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role)
        {
            if (ModelState.IsValid)
            {
                _roleManager.Add(role);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                _roleManager.Update(role);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _roleManager.Delete(id);

            return RedirectToAction("Index");
        }
    }
}