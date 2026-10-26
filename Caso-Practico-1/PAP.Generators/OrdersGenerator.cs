using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Orders already contains data.");
            }

            var customers = _customerRepository.GetAll().ToList();
            var addresses = _addressRepository.GetAll().ToList();

            if (customers.Count == 0) throw new Exception("You must generate Customers first.");
            if (addresses.Count == 0) throw new Exception("You must generate Addresses first.");

            var rnd = GeneratorData.NewRandom();
            string[] statuses = { "Pending", "Paid", "Shipped", "Delivered", "Delivered" };
            var orders = new List<Order>();

            // 1 o 2 órdenes por cliente, enviadas a una dirección DEL MISMO cliente.
            // Los montos quedan en 0 y se calculan al generar OrderItems.
            foreach (var customer in customers)
            {
                var shipping = addresses
                    .Where(a => a.CustomerId == customer.Id)
                    .OrderByDescending(a => a.IsDefault)
                    .FirstOrDefault();

                if (shipping == null) continue;

                int howMany = rnd.Next(1, 3);

                for (int i = 0; i < howMany; i++)
                {
                    DateTime date = GeneratorData.DaysAgo(rnd, 180);

                    orders.Add(new Order
                    {
                        CustomerId = customer.Id,
                        ShippingAddressId = shipping.Id,
                        OrderDate = date,
                        Status = GeneratorData.Pick(rnd, statuses),
                        Subtotal = 0m,
                        TaxAmount = 0m,
                        ShippingCost = 0m,
                        Total = 0m,
                        Notes = rnd.Next(4) == 0 ? "Entregar en horario de oficina." : null,
                        UpdatedAt = date
                    });
                }
            }

            _repository.AddRange(orders);
        }
    }
}