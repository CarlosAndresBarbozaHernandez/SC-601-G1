using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class InventoryManager
    {
        private readonly InventoryRepository _repository;

        public InventoryManager()
        {
            _repository = new InventoryRepository();
        }

        public IEnumerable<Inventory> GetAll()
        {
            return _repository.GetAll();
        }

        public Inventory GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Inventory inventory)
        {
            _repository.Add(inventory);
        }

        public void Update(Inventory inventory)
        {
            _repository.Update(inventory);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}