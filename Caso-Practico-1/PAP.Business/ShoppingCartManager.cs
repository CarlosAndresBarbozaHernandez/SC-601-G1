using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class ShoppingCartManager
    {
        private readonly ShoppingCartRepository _repository;

        public ShoppingCartManager()
        {
            _repository = new ShoppingCartRepository();
        }

        public IEnumerable<ShoppingCart> GetAll()
        {
            return _repository.GetAll();
        }

        public ShoppingCart GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(ShoppingCart shoppingCart)
        {
            _repository.Add(shoppingCart);
        }

        public void Update(ShoppingCart shoppingCart)
        {
            _repository.Update(shoppingCart);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}