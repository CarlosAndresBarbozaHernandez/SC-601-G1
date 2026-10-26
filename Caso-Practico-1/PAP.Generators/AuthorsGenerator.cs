using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Authors already contains data.");
            }

            var rnd = GeneratorData.NewRandom();
            var authors = new List<Author>();

            for (int i = 0; i < 30; i++)
            {
                string country = GeneratorData.Pick(rnd, GeneratorData.Countries);

                authors.Add(new Author
                {
                    FirstName = GeneratorData.Pick(rnd, GeneratorData.FirstNames),
                    LastName = GeneratorData.TwoLastNames(rnd),
                    Biography = "Escritor(a) de " + country + " con obras de ficción, ensayo y divulgación.",
                    BirthDate = GeneratorData.DateBetween(rnd, 1940, 1995),
                    Country = country,
                    CreatedAt = DateTime.Now
                });
            }

            _repository.AddRange(authors);
        }
    }
}