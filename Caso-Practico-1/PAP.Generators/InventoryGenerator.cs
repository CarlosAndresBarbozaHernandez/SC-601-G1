using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class InventoryGenerator
    {
        private readonly InventoryRepository _repository;
        private readonly BookRepository _bookRepository;

        public InventoryGenerator()
        {
            _repository = new InventoryRepository();
            _bookRepository = new BookRepository();
        }

        public bool HasData()
        {
            foreach (var inventory in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Inventory already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            Inventory inventory1 = new Inventory
            {
                BookId = books[0].Id,
                QuantityInStock = 20,
                ReorderLevel = 5,
                LastRestockedAt = DateTime.Now
            };

            _repository.Add(inventory1);

            if (books.Count > 1)
            {
                Inventory inventory2 = new Inventory
                {
                    BookId = books[1].Id,
                    QuantityInStock = 15,
                    ReorderLevel = 5,
                    LastRestockedAt = DateTime.Now
                };

                _repository.Add(inventory2);
            }

            if (books.Count > 2)
            {
                Inventory inventory3 = new Inventory
                {
                    BookId = books[2].Id,
                    QuantityInStock = 30,
                    ReorderLevel = 10,
                    LastRestockedAt = DateTime.Now
                };

                _repository.Add(inventory3);
            }
        }
    }
}