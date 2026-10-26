using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class PurchaseOrderItemsGenerator
    {
        private readonly PurchaseOrderItemRepository _repository;
        private readonly PurchaseOrderRepository _purchaseOrderRepository;
        private readonly BookRepository _bookRepository;

        public PurchaseOrderItemsGenerator()
        {
            _repository = new PurchaseOrderItemRepository();
            _purchaseOrderRepository = new PurchaseOrderRepository();
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
                throw new Exception("PurchaseOrderItems already contains data.");
            }

            var purchaseOrders = _purchaseOrderRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (purchaseOrders.Count == 0) throw new Exception("You must generate PurchaseOrders first.");
            if (books.Count == 0) throw new Exception("Books must contain data first.");

            var rnd = GeneratorData.NewRandom();
            var items = new List<PurchaseOrderItem>();

            // 2 a 4 libros por orden de compra, al 60 % del precio de venta
            foreach (var po in purchaseOrders)
            {
                foreach (var book in GeneratorData.PickDistinct(rnd, books, rnd.Next(2, 5)))
                {
                    items.Add(new PurchaseOrderItem
                    {
                        PurchaseOrderId = po.Id,
                        BookId = book.Id,
                        Quantity = rnd.Next(10, 51),
                        UnitCost = GeneratorData.Money(book.Price * 0.60m)
                    });
                }
            }

            _repository.AddRange(items);

            // Total de cada orden de compra = suma de sus líneas
            foreach (var po in purchaseOrders)
            {
                po.TotalCost = items
                    .Where(x => x.PurchaseOrderId == po.Id)
                    .Sum(x => x.Quantity * x.UnitCost);
            }

            _purchaseOrderRepository.Save();
        }
    }
}