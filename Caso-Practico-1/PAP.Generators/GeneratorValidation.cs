using PAP.Repositories;

namespace PAP.Generators
{
    public class GeneratorValidation
    {
        private readonly BookRepository _bookRepository;
        private readonly AuthorRepository _authorRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly GenreRepository _genreRepository;
        private readonly PublisherRepository _publisherRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly RoleRepository _roleRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly AddressRepository _addressRepository;
        private readonly ShoppingCartRepository _shoppingCartRepository;
        private readonly OrderRepository _orderRepository;
        private readonly PurchaseOrderRepository _purchaseOrderRepository;

        public GeneratorValidation()
        {
            _bookRepository = new BookRepository();
            _authorRepository = new AuthorRepository();
            _categoryRepository = new CategoryRepository();
            _genreRepository = new GenreRepository();
            _publisherRepository = new PublisherRepository();
            _supplierRepository = new SupplierRepository();
            _roleRepository = new RoleRepository();
            _customerRepository = new CustomerRepository();
            _addressRepository = new AddressRepository();
            _shoppingCartRepository = new ShoppingCartRepository();
            _orderRepository = new OrderRepository();
            _purchaseOrderRepository = new PurchaseOrderRepository();
        }

        // Entidades que no dependen de otras tablas
        public bool CanGenerateAuthors()
        {
            return true;
        }

        public bool CanGenerateCategories()
        {
            return true;
        }

        public bool CanGenerateGenres()
        {
            return true;
        }

        public bool CanGeneratePublishers()
        {
            return true;
        }

        public bool CanGenerateSuppliers()
        {
            return true;
        }

        public bool CanGenerateRoles()
        {
            return true;
        }

        public bool CanGenerateCustomers()
        {
            return true;
        }

        // Employees necesita Roles
        public bool CanGenerateEmployees()
        {
            return HasData(_roleRepository.GetAll());
        }

        // Addresses necesita Customers
        public bool CanGenerateAddresses()
        {
            return HasData(_customerRepository.GetAll());
        }

        // ShoppingCarts necesita Customers
        public bool CanGenerateShoppingCarts()
        {
            return HasData(_customerRepository.GetAll());
        }

        // Books necesita Categories, Genres y Publishers
        public bool CanGenerateBooks()
        {
            return HasData(_categoryRepository.GetAll())
                && HasData(_genreRepository.GetAll())
                && HasData(_publisherRepository.GetAll());
        }

        // BookAuthors necesita Books y Authors
        public bool CanGenerateBookAuthors()
        {
            return HasData(_bookRepository.GetAll())
                && HasData(_authorRepository.GetAll());
        }

        // CartItems necesita ShoppingCarts y Books
        public bool CanGenerateCartItems()
        {
            return HasData(_shoppingCartRepository.GetAll())
                && HasData(_bookRepository.GetAll());
        }

        // Inventory necesita Books
        public bool CanGenerateInventory()
        {
            return HasData(_bookRepository.GetAll());
        }

        // Orders necesita Customers y Addresses
        public bool CanGenerateOrders()
        {
            return HasData(_customerRepository.GetAll())
                && HasData(_addressRepository.GetAll());
        }

        // OrderItems necesita Orders y Books
        public bool CanGenerateOrderItems()
        {
            return HasData(_orderRepository.GetAll())
                && HasData(_bookRepository.GetAll());
        }

        // Payments necesita Orders
        public bool CanGeneratePayments()
        {
            return HasData(_orderRepository.GetAll());
        }

        // PurchaseOrders necesita Suppliers
        public bool CanGeneratePurchaseOrders()
        {
            return HasData(_supplierRepository.GetAll());
        }

        // PurchaseOrderItems necesita PurchaseOrders y Books
        public bool CanGeneratePurchaseOrderItems()
        {
            return HasData(_purchaseOrderRepository.GetAll())
                && HasData(_bookRepository.GetAll());
        }

        // Reviews necesita Books y Customers
        public bool CanGenerateReviews()
        {
            return HasData(_bookRepository.GetAll())
                && HasData(_customerRepository.GetAll());
        }

        private bool HasData<T>(System.Collections.Generic.IEnumerable<T> data)
        {
            foreach (T item in data)
            {
                return true;
            }

            return false;
        }
    }
}
