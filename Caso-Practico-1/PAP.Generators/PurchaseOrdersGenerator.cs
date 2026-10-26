using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class PurchaseOrdersGenerator
    {
        private readonly PurchaseOrderRepository _repository;
        private readonly SupplierRepository _supplierRepository;

        public PurchaseOrdersGenerator()
        {
            _repository = new PurchaseOrderRepository();
            _supplierRepository = new SupplierRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("PurchaseOrders already contains data.");
            }

            var suppliers = _supplierRepository.GetAll().ToList();

            if (suppliers.Count == 0) throw new Exception("You must generate Suppliers first.");

            var rnd = GeneratorData.NewRandom();
            string[] statuses = { "Pending", "Shipped", "Received", "Received" };
            var orders = new List<PurchaseOrder>();

            // Dos órdenes de compra por proveedor.
            // TotalCost queda en 0 y se calcula al generar PurchaseOrderItems.
            foreach (var supplier in suppliers)
            {
                for (int i = 0; i < 2; i++)
                {
                    DateTime date = GeneratorData.DaysAgo(rnd, 120);

                    orders.Add(new PurchaseOrder
                    {
                        SupplierId = supplier.Id,
                        OrderDate = date,
                        ExpectedDate = date.AddDays(rnd.Next(7, 21)).Date,
                        Status = GeneratorData.Pick(rnd, statuses),
                        TotalCost = 0m
                    });
                }
            }

            _repository.AddRange(orders);
        }
    }
}