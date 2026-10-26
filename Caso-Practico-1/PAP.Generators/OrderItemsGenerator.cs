using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("OrderItems already contains data.");
            }

            var orders = _orderRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (orders.Count == 0) throw new Exception("You must generate Orders first.");
            if (books.Count == 0) throw new Exception("Books must contain data first.");

            var rnd = GeneratorData.NewRandom();
            var items = new List<OrderItem>();

            // 1 a 4 libros distintos por orden, al precio del libro (10 % de descuento a veces)
            foreach (var order in orders)
            {
                foreach (var book in GeneratorData.PickDistinct(rnd, books, rnd.Next(1, 5)))
                {
                    items.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = book.Id,
                        Quantity = rnd.Next(1, 4),
                        UnitPrice = book.Price,
                        Discount = rnd.Next(5) == 0 ? GeneratorData.Money(book.Price * 0.10m) : 0m
                    });
                }
            }

            _repository.AddRange(items);

            // Montos de cada orden: Subtotal + IVA 13 % + envío (gratis desde $50)
            foreach (var order in orders)
            {
                decimal subtotal = items
                    .Where(x => x.OrderId == order.Id)
                    .Sum(x => x.Quantity * x.UnitPrice - x.Discount);

                order.Subtotal = GeneratorData.Money(subtotal);
                order.TaxAmount = GeneratorData.Money(subtotal * 0.13m);
                order.ShippingCost = subtotal >= 50m ? 0m : 4.50m;
                order.Total = order.Subtotal + order.TaxAmount + order.ShippingCost;
                order.UpdatedAt = DateTime.Now;
            }

            _orderRepository.Save();
        }
    }
}