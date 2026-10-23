using PAP.Business;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class BookAuthorsController : Controller
    {
        private readonly BookManager _bookManager;
        private readonly AuthorManager _authorManager;

        public BookAuthorsController()
        {
            _bookManager = new BookManager();
            _authorManager = new AuthorManager();
        }

        public ActionResult Index(
            int? bookId,
            bool create = false)
        {
            ViewBag.Books = _bookManager.GetAll();
            ViewBag.AllAuthors = _authorManager.GetAll();
            ViewBag.SelectedBookId = bookId;
            ViewBag.ShowCreate = create;

            if (bookId.HasValue)
            {
                var book = _bookManager.GetById(bookId.Value);

                ViewBag.SelectedBook = book;

                var authors = _bookManager.GetAuthors(bookId.Value);

                return View(authors);
            }

            return View(
                new List<PAP.DataAccess.Author>()
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int bookId, int authorId)
        {
            _bookManager.AddAuthor(bookId, authorId);

            return RedirectToAction(
                "Index",
                new { bookId = bookId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int bookId, int authorId)
        {
            _bookManager.RemoveAuthor(bookId, authorId);

            return RedirectToAction(
                "Index",
                new { bookId = bookId }
            );
        }
    }
}