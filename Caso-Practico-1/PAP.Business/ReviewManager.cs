using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class ReviewManager
    {
        private readonly ReviewRepository _repository;

        public ReviewManager()
        {
            _repository = new ReviewRepository();
        }

        public IEnumerable<Review> GetAll()
        {
            return _repository.GetAll();
        }

        public Review GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Review review)
        {
            _repository.Add(review);
        }

        public void Update(Review review)
        {
            _repository.Update(review);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}