using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class AddressManager
    {
        private readonly AddressRepository _repository;

        public AddressManager()
        {
            _repository = new AddressRepository();
        }

        public IEnumerable<Address> GetAll()
        {
            return _repository.GetAll();
        }

        public Address GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Address address)
        {
            _repository.Add(address);
        }

        public void Update(Address address)
        {
            _repository.Update(address);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}