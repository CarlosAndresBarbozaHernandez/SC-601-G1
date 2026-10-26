using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Suppliers already contains data.");
            }

            var rnd = GeneratorData.NewRandom();

            string[] names =
            {
                "Distribuidora Central", "Librería Internacional Mayoreo", "Book Supply CR",
                "Importadora Lectura", "Global Books S.A.", "Papiro Distribuciones",
                "EduLibros", "Mundo Editorial"
            };

            var suppliers = new List<Supplier>();

            for (int i = 0; i < names.Length; i++)
            {
                string[] city = GeneratorData.Pick(rnd, GeneratorData.Cities);

                suppliers.Add(new Supplier
                {
                    Name = names[i],
                    ContactName = GeneratorData.Pick(rnd, GeneratorData.FirstNames) + " " + GeneratorData.TwoLastNames(rnd),
                    Email = "ventas@" + GeneratorData.Slug(names[i]).Replace(".", "") + ".com",
                    Phone = "+506 2" + rnd.Next(100, 1000) + "-" + rnd.Next(1000, 10000),
                    Address = city[0] + ", " + city[1] + ", Costa Rica",
                    IsActive = i != names.Length - 1   // el último queda inactivo
                });
            }

            _repository.AddRange(suppliers);
        }
    }
}