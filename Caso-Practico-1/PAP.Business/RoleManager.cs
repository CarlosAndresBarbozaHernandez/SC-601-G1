using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class RoleManager
    {
        private readonly RoleRepository _repository;

        public RoleManager()
        {
            _repository = new RoleRepository();
        }

        public IEnumerable<Role> GetAll()
        {
            return _repository.GetAll();
        }

        public Role GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Role role)
        {
            _repository.Add(role);
        }

        public void Update(Role role)
        {
            _repository.Update(role);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}