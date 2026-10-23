using PAP.Business;
using PAP.DataAccess;
using PAP.Mvc.ModelBinders;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    [RoutePrefix("biblioteca")]
    public class LibraryController : Controller
    {
        private readonly BookManager _bookManager;

        public LibraryController()
        {
            _bookManager = new BookManager();
        }

        public ActionResult Index(
            bool create = false,
            int? detailsId = null,
            int? editId = null,
            int? deleteId = null)
        {
            // Para el CRUD mostramos todos los libros.
            var books = _bookManager.GetAll();

            ViewBag.ShowCreate = create;

            if (detailsId.HasValue)
            {
                ViewBag.BookDetail =
                    _bookManager.GetById(detailsId.Value);
            }

            if (editId.HasValue)
            {
                ViewBag.BookEdit =
                    _bookManager.GetById(editId.Value);
            }

            if (deleteId.HasValue)
            {
                ViewBag.BookDelete =
                    _bookManager.GetById(deleteId.Value);
            }

            return View(books);
        }

        // Se conserva el ejemplo del profesor.
        public ActionResult Catalog(string vista)
        {
            var books = _bookManager.GetAllBooksFromDataModel();

            if (string.Equals(vista, "cards", StringComparison.OrdinalIgnoreCase))
            {
                return View("Cards", books);
            }

            return View("Index", books);
        }

        // Se conserva el ejemplo de routing.
        [Route("detalle/{id:int}")]
        public ActionResult Details(int id)
        {
            var book = _bookManager.GetById(id);

            if (book == null)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Index", new { detailsId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                book.CreatedAt = DateTime.Now;
                book.UpdatedAt = DateTime.Now;

                if (string.IsNullOrWhiteSpace(book.Name))
                {
                    book.Name = book.Title;
                }

                _bookManager.Add(book);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                book.UpdatedAt = DateTime.Now;

                _bookManager.Update(book);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _bookManager.Delete(id);

            return RedirectToAction("Index");
        }

        // Se conserva el ejemplo de Model Binder personalizado.
        [HttpGet]
        [Route("precio")]
        public ActionResult PrecioDemo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("precio")]
        public ActionResult PrecioDemo(
            [ModelBinder(typeof(CurrencyModelBinder))] decimal precio)
        {
            ViewBag.PrecioConvertido = precio;

            ModelState.Values
                .ToList()
                .ForEach(x =>
                    ViewBag.Errors += string.Join(",", x.Errors));

            return View();
        }
    }
}