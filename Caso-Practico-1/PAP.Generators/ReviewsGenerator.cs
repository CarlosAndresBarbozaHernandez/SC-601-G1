using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class ReviewsGenerator
    {
        private readonly ReviewRepository _repository;
        private readonly BookRepository _bookRepository;
        private readonly CustomerRepository _customerRepository;

        public ReviewsGenerator()
        {
            _repository = new ReviewRepository();
            _bookRepository = new BookRepository();
            _customerRepository = new CustomerRepository();
        }

        public bool HasData()
        {
            foreach (var review in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Reviews already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();
            var customers = _customerRepository.GetAll().ToList();

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            if (customers.Count == 0)
            {
                throw new Exception("You must generate Customers first.");
            }

            Review review1 = new Review
            {
                BookId = books[0].Id,
                CustomerId = customers[0].Id,
                Rating = 5,
                Comment = "Excellent book",
                CreatedAt = DateTime.Now
            };

            _repository.Add(review1);

            if (books.Count > 1)
            {
                Review review2 = new Review
                {
                    BookId = books[1].Id,
                    CustomerId = customers[0].Id,
                    Rating = 4,
                    Comment = "Very good book",
                    CreatedAt = DateTime.Now
                };

                _repository.Add(review2);
            }
        }
    }
}