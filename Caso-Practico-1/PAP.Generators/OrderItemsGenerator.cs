using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class OrderItemsGenerator
    {
        private readonly OrderItemRepository _repository;
        private readonly OrderRepository _orderRepository;
        private readonly BookRepository _bookRepository;

        public OrderItemsGenerator()
        {
            _repository = new OrderItemRepository();
            _orderRepository = new OrderRepository();
            _bookRepository = new BookRepository();
        }

        public bool HasData()
        {
            foreach (var item in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("OrderItems already contains data.");
            }

            var orders = _orderRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (orders.Count == 0)
            {
                throw new Exception("You must generate Orders first.");
            }

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            OrderItem item1 = new OrderItem
            {
                OrderId = orders[0].Id,
                BookId = books[0].Id,
                Quantity = 1,
                UnitPrice = 25m,
                Discount = 0m
            };

            _repository.Add(item1);

            if (books.Count > 1)
            {
                OrderItem item2 = new OrderItem
                {
                    OrderId = orders[0].Id,
                    BookId = books[1].Id,
                    Quantity = 2,
                    UnitPrice = 30m,
                    Discount = 5m
                };

                _repository.Add(item2);
            }
        }
    }
}