using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Business
{
    public class BookManager
    {
        private readonly BookRepository _repository;

        public BookManager()
        {
            _repository = new BookRepository();
        }

        public IEnumerable<Book> GetAll()
        {
            return _repository.GetAll();
        }

        public Book GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Book book)
        {
            _repository.Add(book);
        }

        public void Update(Book book)
        {
            _repository.Update(book);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public IEnumerable<Book> GetAllBooksFromDataModel()
        {
            var books = _repository.GetAll();

            var myEnumerable = books
                .Where(x => x.IsInStock && x.Price > 20)
                .Where(x => x.Name.Contains("a"))
                .Where(x => x.Category == "Fiction")
                .Select(x => new Book
                {
                    Id = x.Id,
                    Name = x.Name,
                    Title = x.Title,
                    Category = x.Category,
                    Publisher = x.Publisher,
                    Author = x.Author,
                    Description = x.Description,
                    PublicationDate = x.PublicationDate,
                    Height = x.Height,
                    Width = x.Width,
                    Price = x.Price,
                    IsBestSeller = x.IsBestSeller,
                    Genre = x.Genre,
                    IsInStock = x.IsInStock,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                });

            foreach (var book in myEnumerable)
            {
                if (book.Price > 50)
                {
                    book.IsBestSeller = true;
                }

                yield return book;
            }
        }

        public IEnumerable<Author> GetAuthors(int bookId)
        {
            return _repository.GetAuthors(bookId);
        }

        public void AddAuthor(int bookId, int authorId)
        {
            _repository.AddAuthor(bookId, authorId);
        }

        public void RemoveAuthor(int bookId, int authorId)
        {
            _repository.RemoveAuthor(bookId, authorId);
        }
    }
}