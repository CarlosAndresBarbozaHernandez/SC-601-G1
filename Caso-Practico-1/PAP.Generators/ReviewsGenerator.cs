using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Reviews already contains data.");
            }

            var books = _bookRepository.GetAll().ToList();
            var customers = _customerRepository.GetAll().ToList();

            if (books.Count == 0) throw new Exception("Books must contain data first.");
            if (customers.Count == 0) throw new Exception("You must generate Customers first.");

            var rnd = GeneratorData.NewRandom();

            // Comentario según el rating (índice = estrellas)
            string[] comments =
            {
                "",
                "No me gustó, la historia es muy lenta.",
                "Regular, esperaba más del final.",
                "Está bien, entretenido para pasar el rato.",
                "Muy bueno, lo recomiendo.",
                "Excelente, de mis favoritos."
            };

            // Más peso en 4 y 5 estrellas
            int[] ratingPool = { 1, 2, 3, 3, 4, 4, 4, 5, 5, 5 };
            var reviews = new List<Review>();

            // Hasta 3 reseñas por libro, cada una de un cliente distinto (UNIQUE BookId + CustomerId)
            foreach (var book in books)
            {
                int howMany = rnd.Next(0, 4);

                foreach (var customer in GeneratorData.PickDistinct(rnd, customers, howMany))
                {
                    int rating = GeneratorData.Pick(rnd, ratingPool);

                    reviews.Add(new Review
                    {
                        BookId = book.Id,
                        CustomerId = customer.Id,
                        Rating = (byte)rating,
                        Comment = comments[rating],
                        CreatedAt = GeneratorData.DaysAgo(rnd, 200)
                    });
                }
            }

            _repository.AddRange(reviews);
        }
    }
}