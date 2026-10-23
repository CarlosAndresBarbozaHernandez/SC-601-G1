using PAP.DataAccess;
using PAP.Repositories;
using System;
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
                throw new Exception("PurchaseOrderItems already contains data.");
            }

            var purchaseOrders = _purchaseOrderRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (purchaseOrders.Count == 0)
            {
                throw new Exception("You must generate PurchaseOrders first.");
            }

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            PurchaseOrderItem item1 = new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrders[0].Id,
                BookId = books[0].Id,
                Quantity = 5,
                UnitCost = 20m
            };

            _repository.Add(item1);

            if (books.Count > 1)
            {
                PurchaseOrderItem item2 = new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[0].Id,
                    BookId = books[1].Id,
                    Quantity = 10,
                    UnitCost = 15m
                };

                _repository.Add(item2);
            }
        }
    }
}