using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class OrderManager
    {
        private readonly OrderRepository _repository;

        public OrderManager()
        {
            _repository = new OrderRepository();
        }

        public IEnumerable<Order> GetAll()
        {
            return _repository.GetAll();
        }

        public Order GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Order order)
        {
            _repository.Add(order);
        }

        public void Update(Order order)
        {
            _repository.Update(order);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}