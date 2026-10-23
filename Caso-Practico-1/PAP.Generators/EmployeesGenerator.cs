using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class EmployeesGenerator
    {
        private readonly EmployeeRepository _repository;
        private readonly RoleRepository _roleRepository;

        public EmployeesGenerator()
        {
            _repository = new EmployeeRepository();
            _roleRepository = new RoleRepository();
        }

        public bool HasData()
        {
            foreach (var employee in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Employees already contains data.");
            }

            // Employees depends on Roles.
            var roles = _roleRepository.GetAll().ToList();

            if (roles.Count == 0)
            {
                throw new Exception("You must generate Roles first.");
            }

            int roleId = roles[0].Id;

            Employee employee1 = new Employee
            {
                RoleId = roleId,
                FirstName = "Carlos",
                LastName = "Mora",
                Email = "carlos@bookstore.com",
                PasswordHash = "1234",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            Employee employee2 = new Employee
            {
                RoleId = roleId,
                FirstName = "Maria",
                LastName = "Rojas",
                Email = "maria@bookstore.com",
                PasswordHash = "1234",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            Employee employee3 = new Employee
            {
                RoleId = roleId,
                FirstName = "Daniel",
                LastName = "Vargas",
                Email = "daniel@bookstore.com",
                PasswordHash = "1234",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _repository.Add(employee1);
            _repository.Add(employee2);
            _repository.Add(employee3);
        }
    }
}