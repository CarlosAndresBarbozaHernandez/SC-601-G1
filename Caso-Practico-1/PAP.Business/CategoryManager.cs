using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class CategoryManager
    {
        private readonly CategoryRepository _repository;

        public CategoryManager()
        {
            _repository = new CategoryRepository();
        }

        public IEnumerable<Category> GetAll()
        {
            return _repository.GetAll();
        }

        public Category GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Category category)
        {
            _repository.Add(category);
        }

        public void Update(Category category)
        {
            _repository.Update(category);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}