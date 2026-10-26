using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class GenresGenerator
    {
        private readonly GenreRepository _repository;

        public GenresGenerator()
        {
            _repository = new GenreRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Genres already contains data.");
            }

            string[] names =
            {
                "Novela", "Fantasía", "Ciencia ficción", "Misterio", "Romance",
                "Terror", "Aventura", "Poesía", "Thriller", "Autoayuda"
            };

            var genres = names
                .Select(n => new Genre { Name = n, Description = "Género " + n.ToLower() })
                .ToList();

            _repository.AddRange(genres);
        }
    }
}