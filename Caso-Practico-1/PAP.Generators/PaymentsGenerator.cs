using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Payments already contains data.");
            }

            var orders = _orderRepository.GetAll().ToList();

            if (orders.Count == 0) throw new Exception("You must generate Orders first.");

            if (orders.All(o => o.Total == 0m))
            {
                throw new Exception("Generate OrderItems first so the orders have a total to pay.");
            }

            var rnd = GeneratorData.NewRandom();
            string[] methods = { "Card", "Card", "SINPE Móvil", "PayPal", "Transfer" };
            var payments = new List<Payment>();
            int n = 1;

            // Un pago por cada orden que ya no está "Pending", por el total exacto
            foreach (var order in orders.Where(o => o.Status != "Pending" && o.Total > 0m))
            {
                payments.Add(new Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = GeneratorData.Pick(rnd, methods),
                    Amount = order.Total,
                    PaymentDate = order.OrderDate.AddMinutes(rnd.Next(5, 240)),
                    Status = "Completed",
                    TransactionRef = "TX-" + DateTime.Now.Year + "-" + (n++).ToString("000000")
                });
            }

            _repository.AddRange(payments);
        }
    }
}