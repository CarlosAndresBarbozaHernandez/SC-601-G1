using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class CategoriesGenerator
    {
        private readonly CategoryRepository _repository;

        public CategoriesGenerator()
        {
            _repository = new CategoryRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Categories already contains data.");
            }

            string[] names =
            {
                "Ficción", "No ficción", "Ciencia", "Historia", "Tecnología",
                "Arte", "Negocios", "Infantil", "Educación", "Biografías"
            };

            var categories = names
                .Select(n => new Category { Name = n, Description = "Libros de " + n.ToLower() })
                .ToList();

            _repository.AddRange(categories);
        }
    }
}