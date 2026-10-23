using PAP.DataAccess;
using PAP.Repositories;
using System;

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
            foreach (var genre in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Genres already contains data.");
            }

            Genre genre1 = new Genre
            {
                Name = "Fantasy",
                Description = "Fantasy books"
            };

            Genre genre2 = new Genre
            {
                Name = "Science Fiction",
                Description = "Science fiction books"
            };

            Genre genre3 = new Genre
            {
                Name = "Mystery",
                Description = "Mystery books"
            };

            _repository.Add(genre1);
            _repository.Add(genre2);
            _repository.Add(genre3);
        }
    }
}