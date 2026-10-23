using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class OrderItemManager
    {
        private readonly OrderItemRepository _repository;

        public OrderItemManager()
        {
            _repository = new OrderItemRepository();
        }

        public IEnumerable<OrderItem> GetAll()
        {
            return _repository.GetAll();
        }

        public OrderItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(OrderItem orderItem)
        {
            _repository.Add(orderItem);
        }

        public void Update(OrderItem orderItem)
        {
            _repository.Update(orderItem);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}