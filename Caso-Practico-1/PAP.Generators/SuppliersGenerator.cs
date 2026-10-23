using PAP.DataAccess;
using PAP.Repositories;
using System;

namespace PAP.Generators
{
    public class SuppliersGenerator
    {
        private readonly SupplierRepository _repository;

        public SuppliersGenerator()
        {
            _repository = new SupplierRepository();
        }

        public bool HasData()
        {
            foreach (var supplier in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Suppliers already contains data.");
            }

            Supplier supplier1 = new Supplier
            {
                Name = "Book Supply CR",
                ContactName = "Carlos Mora",
                Email = "carlos@booksupply.com",
                Phone = "88881111",
                Address = "San Jose",
                IsActive = true
            };

            Supplier supplier2 = new Supplier
            {
                Name = "Central Books",
                ContactName = "Maria Lopez",
                Email = "maria@centralbooks.com",
                Phone = "88882222",
                Address = "Heredia",
                IsActive = true
            };

            Supplier supplier3 = new Supplier
            {
                Name = "Global Books",
                ContactName = "Daniel Rojas",
                Email = "daniel@globalbooks.com",
                Phone = "88883333",
                Address = "Cartago",
                IsActive = true
            };

            _repository.Add(supplier1);
            _repository.Add(supplier2);
            _repository.Add(supplier3);
        }
    }
}
