using PAP.DataAccess;
using PAP.Repositories;
using System;
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
            foreach (var purchaseOrder in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("PurchaseOrders already contains data.");
            }

            var suppliers = _supplierRepository.GetAll().ToList();

            if (suppliers.Count == 0)
            {
                throw new Exception("You must generate Suppliers first.");
            }

            PurchaseOrder purchaseOrder1 = new PurchaseOrder
            {
                SupplierId = suppliers[0].Id,
                OrderDate = DateTime.Now,
                ExpectedDate = DateTime.Now.AddDays(7),
                Status = "Pending",
                TotalCost = 250m
            };

            _repository.Add(purchaseOrder1);

            if (suppliers.Count > 1)
            {
                PurchaseOrder purchaseOrder2 = new PurchaseOrder
                {
                    SupplierId = suppliers[1].Id,
                    OrderDate = DateTime.Now,
                    ExpectedDate = DateTime.Now.AddDays(10),
                    Status = "Pending",
                    TotalCost = 400m
                };

                _repository.Add(purchaseOrder2);
            }
        }
    }
}