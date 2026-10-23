using PAP.DataAccess;
using PAP.Repositories;
using System;

namespace PAP.Generators
{
    public class AuthorsGenerator
    {
        private readonly AuthorRepository _repository;

        public AuthorsGenerator()
        {
            _repository = new AuthorRepository();
        }

        public bool HasData()
        {
            foreach (var author in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Authors already contains data.");
            }

            Author author1 = new Author
            {
                FirstName = "Gabriel",
                LastName = "Garcia Marquez",
                Biography = "Colombian writer",
                BirthDate = new DateTime(1927, 3, 6),
                Country = "Colombia",
                CreatedAt = DateTime.Now
            };

            Author author2 = new Author
            {
                FirstName = "George",
                LastName = "Orwell",
                Biography = "English writer",
                BirthDate = new DateTime(1903, 6, 25),
                Country = "United Kingdom",
                CreatedAt = DateTime.Now
            };

            Author author3 = new Author
            {
                FirstName = "Jane",
                LastName = "Austen",
                Biography = "English novelist",
                BirthDate = new DateTime(1775, 12, 16),
                Country = "United Kingdom",
                CreatedAt = DateTime.Now
            };

            _repository.Add(author1);
            _repository.Add(author2);
            _repository.Add(author3);
        }
    }
}