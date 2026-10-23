using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class EmployeeManager
    {
        private readonly EmployeeRepository _repository;

        public EmployeeManager()
        {
            _repository = new EmployeeRepository();
        }

        public IEnumerable<Employee> GetAll()
        {
            return _repository.GetAll();
        }

        public Employee GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Employee employee)
        {
            _repository.Add(employee);
        }

        public void Update(Employee employee)
        {
            _repository.Update(employee);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}