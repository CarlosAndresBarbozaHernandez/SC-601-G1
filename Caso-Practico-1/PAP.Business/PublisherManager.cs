using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class PublisherManager
    {
        private readonly PublisherRepository _repository;

        public PublisherManager()
        {
            _repository = new PublisherRepository();
        }

        public IEnumerable<Publisher> GetAll()
        {
            return _repository.GetAll();
        }

        public Publisher GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Publisher publisher)
        {
            _repository.Add(publisher);
        }

        public void Update(Publisher publisher)
        {
            _repository.Update(publisher);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}