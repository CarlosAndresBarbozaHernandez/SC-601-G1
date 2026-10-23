using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class PaymentsGenerator
    {
        private readonly PaymentRepository _repository;
        private readonly OrderRepository _orderRepository;

        public PaymentsGenerator()
        {
            _repository = new PaymentRepository();
            _orderRepository = new OrderRepository();
        }

        public bool HasData()
        {
            foreach (var payment in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Payments already contains data.");
            }

            var orders = _orderRepository.GetAll().ToList();

            if (orders.Count == 0)
            {
                throw new Exception("You must generate Orders first.");
            }

            Payment payment1 = new Payment
            {
                OrderId = orders[0].Id,
                PaymentMethod = "Card",
                Amount = orders[0].Total,
                PaymentDate = DateTime.Now,
                Status = "Completed",
                TransactionRef = "PAY001"
            };

            _repository.Add(payment1);
        }
    }
}