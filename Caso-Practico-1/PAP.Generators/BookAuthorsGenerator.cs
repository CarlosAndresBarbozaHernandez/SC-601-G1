using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class BookAuthorsGenerator
    {
        private readonly BookRepository _bookRepository;
        private readonly AuthorRepository _authorRepository;

        public BookAuthorsGenerator()
        {
            _bookRepository = new BookRepository();
            _authorRepository = new AuthorRepository();
        }

        public bool HasData()
        {
            var books = _bookRepository.GetAll().ToList();

            foreach (var book in books)
            {
                if (book.Authors.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("BookAuthors already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();
            var authors = _authorRepository.GetAll().ToList();

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            if (authors.Count == 0)
            {
                throw new Exception("You must generate Authors first.");
            }

            _bookRepository.AddAuthor(
                books[0].Id,
                authors[0].Id
            );

            if (books.Count > 1 && authors.Count > 1)
            {
                _bookRepository.AddAuthor(
                    books[1].Id,
                    authors[1].Id
                );
            }

            if (books.Count > 2 && authors.Count > 2)
            {
                _bookRepository.AddAuthor(
                    books[2].Id,
                    authors[2].Id
                );
            }
        }
    }
}