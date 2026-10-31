using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _bookRepository.GetAll().Any(b => b.Authors.Count > 0);
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("BookAuthors already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();
            var authors = _authorRepository.GetAll().ToList();

            if (books.Count == 0) throw new Exception("Books must contain data first.");
            if (authors.Count == 0) throw new Exception("You must generate Authors first.");

            var rnd = GeneratorData.NewRandom();

            // Cada libro: 1 autor (70 %) o 2 autores (30 %)
            foreach (var book in books)
            {
                int howMany = rnd.Next(10) < 7 ? 1 : 2;
                var chosen = GeneratorData.PickDistinct(rnd, authors, howMany);

                foreach (var author in chosen)
                {
                    _bookRepository.AddAuthor(book.Id, author.Id);
                }

                // Llena la columna de texto "Author" del libro para que se vea en la vista de Books
                var tracked = _bookRepository.GetById(book.Id);
                tracked.Author = string.Join(", ", chosen.Select(a => a.FirstName + " " + a.LastName.Split(' ')[0]));
                tracked.UpdatedAt = DateTime.Now;
            }

            _bookRepository.Save();
        }
    }
}