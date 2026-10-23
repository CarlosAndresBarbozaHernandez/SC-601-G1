using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class PurchaseOrderManager
    {
        private readonly PurchaseOrderRepository _repository;

        public PurchaseOrderManager()
        {
            _repository = new PurchaseOrderRepository();
        }

        public IEnumerable<PurchaseOrder> GetAll()
        {
            return _repository.GetAll();
        }

        public PurchaseOrder GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(PurchaseOrder purchaseOrder)
        {
            _repository.Add(purchaseOrder);
        }

        public void Update(PurchaseOrder purchaseOrder)
        {
            _repository.Update(purchaseOrder);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}