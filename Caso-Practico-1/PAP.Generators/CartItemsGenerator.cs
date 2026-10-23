using PAP.DataAccess;
using PAP.Repositories;
using System;
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
            foreach (var item in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("CartItems already contains data.");
            }

            var carts = _cartRepository.GetAll().ToList();
            var books = _bookRepository.GetAll().ToList();

            if (carts.Count == 0)
            {
                throw new Exception("You must generate ShoppingCarts first.");
            }

            if (books.Count == 0)
            {
                throw new Exception("Books must contain data first.");
            }

            CartItem item1 = new CartItem
            {
                CartId = carts[0].Id,
                BookId = books[0].Id,
                Quantity = 1,
                AddedAt = DateTime.Now
            };

            _repository.Add(item1);

            if (books.Count > 1)
            {
                CartItem item2 = new CartItem
                {
                    CartId = carts[0].Id,
                    BookId = books[1].Id,
                    Quantity = 2,
                    AddedAt = DateTime.Now
                };

                _repository.Add(item2);
            }

            if (carts.Count > 1 && books.Count > 2)
            {
                CartItem item3 = new CartItem
                {
                    CartId = carts[1].Id,
                    BookId = books[2].Id,
                    Quantity = 1,
                    AddedAt = DateTime.Now
                };

                _repository.Add(item3);
            }
        }
    }
}