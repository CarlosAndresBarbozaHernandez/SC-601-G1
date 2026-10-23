using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class SupplierManager
    {
        private readonly SupplierRepository _repository;

        public SupplierManager()
        {
            _repository = new SupplierRepository();
        }

        public IEnumerable<Supplier> GetAll()
        {
            return _repository.GetAll();
        }

        public Supplier GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Supplier supplier)
        {
            _repository.Add(supplier);
        }

        public void Update(Supplier supplier)
        {
            _repository.Update(supplier);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}