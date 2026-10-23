using PAP.DataAccess;
using PAP.Repositories;
using System;

namespace PAP.Generators
{
    public class RolesGenerator
    {
        private readonly RoleRepository _repository;

        public RolesGenerator()
        {
            _repository = new RoleRepository();
        }

        public bool HasData()
        {
            foreach (var role in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Roles already contains data.");
            }

            Role role1 = new Role
            {
                Name = "Admin"
            };

            Role role2 = new Role
            {
                Name = "Manager"
            };

            Role role3 = new Role
            {
                Name = "Cashier"
            };

            _repository.Add(role1);
            _repository.Add(role2);
            _repository.Add(role3);
        }
    }
}