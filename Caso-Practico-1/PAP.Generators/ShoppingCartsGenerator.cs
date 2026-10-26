using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class ShoppingCartsGenerator
    {
        private readonly ShoppingCartRepository _repository;
        private readonly CustomerRepository _customerRepository;

        public ShoppingCartsGenerator()
        {
            _repository = new ShoppingCartRepository();
            _customerRepository = new CustomerRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("ShoppingCarts already contains data.");
            }

            var customers = _customerRepository.GetAll().ToList();

            if (customers.Count == 0) throw new Exception("You must generate Customers first.");

            var rnd = GeneratorData.NewRandom();

            // Un carrito por cliente
            var carts = customers.Select(c =>
            {
                DateTime created = GeneratorData.DaysAgo(rnd, 60);

                return new ShoppingCart
                {
                    CustomerId = c.Id,
                    CreatedAt = created,
                    UpdatedAt = created.AddDays(rnd.Next(0, 5))
                };
            }).ToList();

            _repository.AddRange(carts);
        }
    }
}