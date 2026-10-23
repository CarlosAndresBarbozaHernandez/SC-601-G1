using PAP.DataAccess;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Repositories
{
    public class BookRepository : RepositoryBase<Book>
    {
        public IEnumerable<Book> MyBooks()
        {
            var test = from x in _context.Books
                       where !x.IsInStock && x.Name.Contains("a")
                       select x;

            return test.ToList();
        }

        public IEnumerable<Author> GetAuthors(int bookId)
        {
            var book = _context.Books.Find(bookId);

            if (book != null)
            {
                return book.Authors.ToList();
            }

            return new List<Author>();
        }

        public void AddAuthor(int bookId, int authorId)
        {
            var book = _context.Books.Find(bookId);
            var author = _context.Authors.Find(authorId);

            if (book != null && author != null)
            {
                if (!book.Authors.Any(x => x.Id == authorId))
                {
                    book.Authors.Add(author);
                    _context.SaveChanges();
                }
            }
        }

        public void RemoveAuthor(int bookId, int authorId)
        {
            var book = _context.Books.Find(bookId);
            var author = _context.Authors.Find(authorId);

            if (book != null && author != null)
            {
                if (book.Authors.Any(x => x.Id == authorId))
                {
                    book.Authors.Remove(author);
                    _context.SaveChanges();
                }
            }
        }
    }
}