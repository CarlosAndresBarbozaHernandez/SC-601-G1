using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class CartItemsGenerator
    {
        private readonly CartItemRepository _repository;
        private readonly ShoppingCartRepository _cartRepository;
        private readonly BookRepository _bookRepository;

        public CartItemsGenerator()
        {
            _repository = new CartItemRepository();
            _cartRepository = new ShoppingCartRepository();
            _bookRepository = new BookRepository();
        }

        public bool HasData()
        {
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("CartItems already contains data.");
            }

            var carts = _cartRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (carts.Count == 0) throw new Exception("You must generate ShoppingCarts first.");
            if (books.Count == 0) throw new Exception("Books must contain data first.");

            var rnd = GeneratorData.NewRandom();
            var items = new List<CartItem>();

            // 0 a 3 libros distintos por carrito (UNIQUE CartId + BookId)
            foreach (var cart in carts)
            {
                foreach (var book in GeneratorData.PickDistinct(rnd, books, rnd.Next(0, 4)))
                {
                    items.Add(new CartItem
                    {
                        CartId = cart.Id,
                        BookId = book.Id,
                        Quantity = rnd.Next(1, 4),
                        AddedAt = cart.CreatedAt.AddHours(rnd.Next(1, 72))
                    });
                }
            }

            _repository.AddRange(items);
        }
    }
}