using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class CartItemManager
    {
        private readonly CartItemRepository _repository;

        public CartItemManager()
        {
            _repository = new CartItemRepository();
        }

        public IEnumerable<CartItem> GetAll()
        {
            return _repository.GetAll();
        }

        public CartItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(CartItem cartItem)
        {
            _repository.Add(cartItem);
        }

        public void Update(CartItem cartItem)
        {
            _repository.Update(cartItem);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}