using PAP.DataAccess;
using PAP.Repositories;
using System;
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
            foreach (var cart in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("ShoppingCarts already contains data.");
            }

            // ShoppingCarts depends on Customers.
            var customers = _customerRepository.GetAll().ToList();

            if (customers.Count == 0)
            {
                throw new Exception("You must generate Customers first.");
            }

            ShoppingCart cart1 = new ShoppingCart
            {
                CustomerId = customers[0].Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _repository.Add(cart1);

            if (customers.Count > 1)
            {
                ShoppingCart cart2 = new ShoppingCart
                {
                    CustomerId = customers[1].Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _repository.Add(cart2);
            }

            if (customers.Count > 2)
            {
                ShoppingCart cart3 = new ShoppingCart
                {
                    CustomerId = customers[2].Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _repository.Add(cart3);
            }
        }
    }
}