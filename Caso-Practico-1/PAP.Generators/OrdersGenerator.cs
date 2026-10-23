using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class OrdersGenerator
    {
        private readonly OrderRepository _repository;
        private readonly CustomerRepository _customerRepository;
        private readonly AddressRepository _addressRepository;

        public OrdersGenerator()
        {
            _repository = new OrderRepository();
            _customerRepository = new CustomerRepository();
            _addressRepository = new AddressRepository();
        }

        public bool HasData()
        {
            foreach (var order in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Orders already contains data.");
            }

            var customers = _customerRepository.GetAll().ToList();
            var addresses = _addressRepository.GetAll().ToList();

            if (customers.Count == 0)
            {
                throw new Exception("You must generate Customers first.");
            }

            if (addresses.Count == 0)
            {
                throw new Exception("You must generate Addresses first.");
            }

            var address = addresses[0];

            Order order1 = new Order
            {
                CustomerId = address.CustomerId,
                ShippingAddressId = address.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Subtotal = 100m,
                TaxAmount = 13m,
                ShippingCost = 5m,
                Total = 118m,
                Notes = "Generated order",
                UpdatedAt = DateTime.Now
            };

            _repository.Add(order1);
        }
    }
}