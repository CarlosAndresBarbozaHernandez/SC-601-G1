using PAP.DataAccess;
using PAP.Repositories;
using System;

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
            foreach (var category in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Categories already contains data.");
            }

            Category category1 = new Category
            {
                Name = "Fiction",
                Description = "Fiction books"
            };

            Category category2 = new Category
            {
                Name = "Technology",
                Description = "Technology and programming books"
            };

            Category category3 = new Category
            {
                Name = "History",
                Description = "History books"
            };

            _repository.Add(category1);
            _repository.Add(category2);
            _repository.Add(category3);
        }
    }
}