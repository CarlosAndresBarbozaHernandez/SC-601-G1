using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Employees already contains data.");
            }

            var roles = _roleRepository.GetAll().ToList();

            if (roles.Count == 0) throw new Exception("You must generate Roles first.");

            var rnd = GeneratorData.NewRandom();
            var employees = new List<Employee>();

            // 1) Integrantes del grupo como administradores
            foreach (var member in GeneratorData.TeamMembers)
            {
                employees.Add(NewEmployee(member[0], member[1], RoleId(roles, "Admin", 0)));
            }

            // 2) Dos Managers y seis Cashiers
            for (int i = 0; i < 8; i++)
            {
                int roleId = i < 2 ? RoleId(roles, "Manager", 1) : RoleId(roles, "Cashier", 2);

                employees.Add(NewEmployee(
                    GeneratorData.Pick(rnd, GeneratorData.FirstNames),
                    GeneratorData.TwoLastNames(rnd),
                    roleId));
            }

            // 3) Correos únicos: nombre.apellido@bookstore.com (+ número si se repite)
            var used = new HashSet<string>();

            foreach (var e in employees)
            {
                string baseEmail = GeneratorData.Slug(e.FirstName.Split(' ')[0] + " " + e.LastName.Split(' ')[0]);
                string email = baseEmail;
                int n = 2;

                while (!used.Add(email))
                {
                    email = baseEmail + n++;
                }

                e.Email = email + "@bookstore.com";
            }

            _repository.AddRange(employees);
        }

        /// <summary>Busca el Id del rol por nombre; si no existe usa uno por posición.</summary>
        private static int RoleId(List<Role> roles, string name, int fallbackIndex)
        {
            var role = roles.FirstOrDefault(r => r.Name == name);
            return role != null ? role.Id : roles[fallbackIndex % roles.Count].Id;
        }

        private static Employee NewEmployee(string first, string last, int roleId)
        {
            return new Employee
            {
                RoleId = roleId,
                FirstName = first,
                LastName = last,
                PasswordHash = "HASH_" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }
    }
}