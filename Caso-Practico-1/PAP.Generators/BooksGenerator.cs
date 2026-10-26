using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class BooksGenerator
    {
        private readonly BookRepository _repository;
        private readonly CategoryRepository _categoryRepository;
        private readonly GenreRepository _genreRepository;
        private readonly PublisherRepository _publisherRepository;

        public BooksGenerator()
        {
            _repository = new BookRepository();
            _categoryRepository = new CategoryRepository();
            _genreRepository = new GenreRepository();
            _publisherRepository = new PublisherRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Books already contains data.");
            }

            // Tablas padre (llaves foráneas)
            var categories = _categoryRepository.GetAll().ToList();
            var genres = _genreRepository.GetAll().ToList();
            var publishers = _publisherRepository.GetAll().ToList();

            if (categories.Count == 0) throw new Exception("You must generate Categories first.");
            if (genres.Count == 0) throw new Exception("You must generate Genres first.");
            if (publishers.Count == 0) throw new Exception("You must generate Publishers first.");

            var rnd = GeneratorData.NewRandom();
            var books = new List<Book>();
            string[] languages = { "Español", "Español", "Español", "Inglés" };

            for (int i = 0; i < GeneratorData.BookTitles.Length; i++)
            {
                string title = GeneratorData.BookTitles[i];
                var category = GeneratorData.Pick(rnd, categories);
                var genre = GeneratorData.Pick(rnd, genres);
                var publisher = GeneratorData.Pick(rnd, publishers);

                books.Add(new Book
                {
                    Name = title,
                    Title = title,
                    Description = "Una obra que explora " + title.ToLower() + ".",
                    PublicationDate = GeneratorData.DateBetween(rnd, 1995, 2025),
                    Price = GeneratorData.Money(rnd.Next(8, 46) + (decimal)rnd.Next(0, 100) / 100),
                    IsBestSeller = rnd.Next(5) == 0,     // ~20 % best sellers
                    IsInStock = rnd.Next(8) != 0,        // ~87 % en stock
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,

                    // Llaves foráneas + columnas de texto (para las vistas viejas)
                    PublisherId = publisher.Id,
                    Publisher = publisher.Name,
                    CategoryId = category.Id,
                    Category = category.Name,
                    GenreId = genre.Id,
                    Genre = genre.Name,

                    ISBN = "978-99977-" + (i + 1).ToString("0000"),
                    Pages = rnd.Next(90, 651),
                    Language = GeneratorData.Pick(rnd, languages)
                });
            }

            _repository.AddRange(books);
        }
    }
}
