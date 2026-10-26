using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Roles already contains data.");
            }

            var roles = new List<Role>
            {
                new Role { Name = "Admin" },
                new Role { Name = "Manager" },
                new Role { Name = "Cashier" }
            };

            _repository.AddRange(roles);
        }
    }
}