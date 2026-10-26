using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Inventory already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();

            if (books.Count == 0) throw new Exception("Books must contain data first.");

            var rnd = GeneratorData.NewRandom();

            // Un registro por libro. Los libros "sin stock" quedan en 0.
            var inventory = books.Select(b => new Inventory
            {
                BookId = b.Id,
                QuantityInStock = b.IsInStock ? rnd.Next(1, 61) : 0,
                ReorderLevel = rnd.Next(5, 11),
                LastRestockedAt = GeneratorData.DaysAgo(rnd, 90)
            }).ToList();

            _repository.AddRange(inventory);
        }
    }
}