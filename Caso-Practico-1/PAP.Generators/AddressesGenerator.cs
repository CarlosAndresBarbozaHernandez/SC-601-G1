using PAP.DataAccess;
using PAP.Repositories;
using System;
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
            foreach (var address in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Addresses already contains data.");
            }

            // Addresses depends on Customers.
            var customers = _customerRepository.GetAll().ToList();

            if (customers.Count == 0)
            {
                throw new Exception("You must generate Customers first.");
            }

            int customerId = customers[0].Id;

            Address address1 = new Address
            {
                CustomerId = customerId,
                AddressType = "Home",
                Street = "Calle Principal",
                City = "Heredia",
                State = "Heredia",
                PostalCode = "40101",
                Country = "Costa Rica",
                IsDefault = true
            };

            Address address2 = new Address
            {
                CustomerId = customerId,
                AddressType = "Work",
                Street = "Avenida Central",
                City = "San Jose",
                State = "San Jose",
                PostalCode = "10101",
                Country = "Costa Rica",
                IsDefault = false
            };

            Address address3 = new Address
            {
                CustomerId = customerId,
                AddressType = "Other",
                Street = "Calle Segunda",
                City = "Cartago",
                State = "Cartago",
                PostalCode = "30101",
                Country = "Costa Rica",
                IsDefault = false
            };

            _repository.Add(address1);
            _repository.Add(address2);
            _repository.Add(address3);
        }
    }
}