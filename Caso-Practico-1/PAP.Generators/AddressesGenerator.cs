using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class AddressesGenerator
    {
        private readonly AddressRepository _repository;
        private readonly CustomerRepository _customerRepository;

        public AddressesGenerator()
        {
            _repository = new AddressRepository();
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
                throw new Exception("Addresses already contains data.");
            }

            var customers = _customerRepository.GetAll().ToList();

            if (customers.Count == 0) throw new Exception("You must generate Customers first.");

            var rnd = GeneratorData.NewRandom();
            var addresses = new List<Address>();

            // Cada cliente: una dirección de envío (default) y una de facturación
            foreach (var customer in customers)
            {
                addresses.Add(NewAddress(rnd, customer.Id, "Shipping", true));
                addresses.Add(NewAddress(rnd, customer.Id, "Billing", false));
            }

            _repository.AddRange(addresses);
        }

        private static Address NewAddress(Random rnd, int customerId, string type, bool isDefault)
        {
            string[] city = GeneratorData.Pick(rnd, GeneratorData.Cities);

            return new Address
            {
                CustomerId = customerId,
                AddressType = type,
                Street = "Calle " + rnd.Next(1, 90) + ", casa " + rnd.Next(1, 50),
                City = city[0],
                State = city[1],
                PostalCode = city[2],
                Country = "Costa Rica",
                IsDefault = isDefault
            };
        }
    }
}