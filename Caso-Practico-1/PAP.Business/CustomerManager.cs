using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class CustomerManager
    {
        private readonly CustomerRepository _repository;

        public CustomerManager()
        {
            _repository = new CustomerRepository();
        }

        public IEnumerable<Customer> GetAll()
        {
            return _repository.GetAll();
        }

        public Customer GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Customer customer)
        {
            _repository.Add(customer);
        }

        public void Update(Customer customer)
        {
            _repository.Update(customer);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}