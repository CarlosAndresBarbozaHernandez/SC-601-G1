using PAP.DataAccess;
using PAP.Repositories;
using System;
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
            foreach (var book in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Books already contains data.");
            }

            var categories = _categoryRepository.GetAll().ToList();
            var genres = _genreRepository.GetAll().ToList();
            var publishers = _publisherRepository.GetAll().ToList();

            if (categories.Count == 0)
            {
                throw new Exception("You must generate Categories first.");
            }

            if (genres.Count == 0)
            {
                throw new Exception("You must generate Genres first.");
            }

            if (publishers.Count == 0)
            {
                throw new Exception("You must generate Publishers first.");
            }

            Book book1 = new Book
            {
                Name = "Generated Book 1",
                Title = "Generated Book 1",
                Category = categories[0].Name,
                Publisher = publishers[0].Name,
                Author = "Generated Author",
                Description = "Automatically generated book",
                PublicationDate = new DateTime(2025, 1, 1),
                Price = 25m,
                IsBestSeller = false,
                Genre = genres[0].Name,
                IsInStock = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PublisherId = publishers[0].Id,
                CategoryId = categories[0].Id,
                GenreId = genres[0].Id,
                ISBN = "9780000000001",
                Pages = 250,
                Language = "English"
            };

            Book book2 = new Book
            {
                Name = "Generated Book 2",
                Title = "Generated Book 2",
                Category = categories[0].Name,
                Publisher = publishers[0].Name,
                Author = "Generated Author",
                Description = "Automatically generated book",
                PublicationDate = new DateTime(2025, 2, 1),
                Price = 30m,
                IsBestSeller = true,
                Genre = genres[0].Name,
                IsInStock = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PublisherId = publishers[0].Id,
                CategoryId = categories[0].Id,
                GenreId = genres[0].Id,
                ISBN = "9780000000002",
                Pages = 300,
                Language = "English"
            };

            _repository.Add(book1);
            _repository.Add(book2);
        }
    }
}