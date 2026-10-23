using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class PurchaseOrderItemManager
    {
        private readonly PurchaseOrderItemRepository _repository;

        public PurchaseOrderItemManager()
        {
            _repository = new PurchaseOrderItemRepository();
        }

        public IEnumerable<PurchaseOrderItem> GetAll()
        {
            return _repository.GetAll();
        }

        public PurchaseOrderItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(PurchaseOrderItem purchaseOrderItem)
        {
            _repository.Add(purchaseOrderItem);
        }

        public void Update(PurchaseOrderItem purchaseOrderItem)
        {
            _repository.Update(purchaseOrderItem);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}