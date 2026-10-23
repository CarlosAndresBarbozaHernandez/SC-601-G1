using PAP.DataAccess;
using PAP.Repositories;
using System;

namespace PAP.Generators
{
    public class CustomersGenerator
    {
        private readonly CustomerRepository _repository;

        public CustomersGenerator()
        {
            _repository = new CustomerRepository();
        }

        public bool HasData()
        {
            foreach (var customer in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Customers already contains data.");
            }

            Customer customer1 = new Customer
            {
                FirstName = "Sebastian",
                LastName = "Rivera",
                Email = "sebastian@test.com",
                PasswordHash = "1234",
                Phone = "88888888",
                BirthDate = new DateTime(2005, 5, 10),
                IsActive = true
            };

            Customer customer2 = new Customer
            {
                FirstName = "Andrea",
                LastName = "Mora",
                Email = "andrea@test.com",
                PasswordHash = "1234",
                Phone = "87777777",
                BirthDate = new DateTime(2000, 8, 15),
                IsActive = true
            };

            Customer customer3 = new Customer
            {
                FirstName = "Daniel",
                LastName = "Rojas",
                Email = "daniel@test.com",
                PasswordHash = "1234",
                Phone = "86666666",
                BirthDate = new DateTime(1998, 3, 20),
                IsActive = true
            };

            _repository.Add(customer1);
            _repository.Add(customer2);
            _repository.Add(customer3);
        }
    }
}