using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Customers already contains data.");
            }

            var rnd = GeneratorData.NewRandom();
            var customers = new List<Customer>();

            // 1) Integrantes del grupo
            foreach (var member in GeneratorData.TeamMembers)
            {
                string email = GeneratorData.Slug(member[0].Split(' ')[0] + " " + member[1].Split(' ')[0]) + "@correo.com";
                customers.Add(NewCustomer(rnd, member[0], member[1], email));
            }

            // 2) Clientes generados hasta completar 40
            for (int i = customers.Count + 1; i <= 40; i++)
            {
                string first = GeneratorData.Pick(rnd, GeneratorData.FirstNames);
                string last = GeneratorData.TwoLastNames(rnd);
                string email = GeneratorData.Slug(first + " " + last.Split(' ')[0]) + i + "@correo.com";

                customers.Add(NewCustomer(rnd, first, last, email));
            }

            _repository.AddRange(customers);
        }

        private static Customer NewCustomer(Random rnd, string first, string last, string email)
        {
            return new Customer
            {
                FirstName = first,
                LastName = last,
                Email = email,
                PasswordHash = "HASH_" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper(),
                Phone = GeneratorData.Phone(rnd),
                BirthDate = GeneratorData.DateBetween(rnd, 1965, 2005),
                IsActive = rnd.Next(10) != 0,   // ~90 % activos
                CreatedAt = GeneratorData.DaysAgo(rnd, 365),
                UpdatedAt = DateTime.Now
            };
        }
    }
}