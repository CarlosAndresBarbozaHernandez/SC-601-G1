using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class PaymentManager
    {
        private readonly PaymentRepository _repository;

        public PaymentManager()
        {
            _repository = new PaymentRepository();
        }

        public IEnumerable<Payment> GetAll()
        {
            return _repository.GetAll();
        }

        public Payment GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Payment payment)
        {
            _repository.Add(payment);
        }

        public void Update(Payment payment)
        {
            _repository.Update(payment);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}