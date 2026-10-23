using PAP.Generators;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly AuthorsGenerator _authorsGenerator;
        private readonly CategoriesGenerator _categoriesGenerator;
        private readonly GenresGenerator _genresGenerator;
        private readonly PublishersGenerator _publishersGenerator;
        private readonly SuppliersGenerator _suppliersGenerator;
        private readonly RolesGenerator _rolesGenerator;
        private readonly CustomersGenerator _customersGenerator;
        private readonly EmployeesGenerator _employeesGenerator;
        private readonly AddressesGenerator _addressesGenerator;
        private readonly ShoppingCartsGenerator _shoppingCartsGenerator;
        private readonly CartItemsGenerator _cartItemsGenerator;
        private readonly InventoryGenerator _inventoryGenerator;
        private readonly OrdersGenerator _ordersGenerator;
        private readonly OrderItemsGenerator _orderItemsGenerator;
        private readonly PaymentsGenerator _paymentsGenerator;
        private readonly PurchaseOrdersGenerator _purchaseOrdersGenerator;
        private readonly PurchaseOrderItemsGenerator _purchaseOrderItemsGenerator;
        private readonly ReviewsGenerator _reviewsGenerator;
        private readonly BooksGenerator _booksGenerator;
        private readonly BookAuthorsGenerator _bookAuthorsGenerator;

        private readonly GeneratorValidation _validation;

        public AdminController()
        {
            _authorsGenerator = new AuthorsGenerator();
            _categoriesGenerator = new CategoriesGenerator();
            _genresGenerator = new GenresGenerator();
            _publishersGenerator = new PublishersGenerator();
            _suppliersGenerator = new SuppliersGenerator();
            _rolesGenerator = new RolesGenerator();
            _customersGenerator = new CustomersGenerator();
            _employeesGenerator = new EmployeesGenerator();
            _addressesGenerator = new AddressesGenerator();
            _shoppingCartsGenerator = new ShoppingCartsGenerator();
            _cartItemsGenerator = new CartItemsGenerator();
            _inventoryGenerator = new InventoryGenerator();
            _ordersGenerator = new OrdersGenerator();
            _orderItemsGenerator = new OrderItemsGenerator();
            _paymentsGenerator = new PaymentsGenerator();
            _purchaseOrdersGenerator = new PurchaseOrdersGenerator();
            _purchaseOrderItemsGenerator = new PurchaseOrderItemsGenerator();
            _reviewsGenerator = new ReviewsGenerator();
            _booksGenerator = new BooksGenerator();
            _bookAuthorsGenerator = new BookAuthorsGenerator();

            _validation = new GeneratorValidation();
        }

        public ActionResult Index()
        {
            ViewBag.CanGenerateAuthors =
                _validation.CanGenerateAuthors();

            ViewBag.CanGenerateCategories =
                _validation.CanGenerateCategories();

            ViewBag.CanGenerateGenres =
                _validation.CanGenerateGenres();

            ViewBag.CanGeneratePublishers =
                _validation.CanGeneratePublishers();

            ViewBag.CanGenerateSuppliers =
                _validation.CanGenerateSuppliers();

            ViewBag.CanGenerateRoles =
                _validation.CanGenerateRoles();

            ViewBag.CanGenerateCustomers =
                _validation.CanGenerateCustomers();

            ViewBag.CanGenerateEmployees =
                _validation.CanGenerateEmployees();

            ViewBag.CanGenerateAddresses =
                _validation.CanGenerateAddresses();

            ViewBag.CanGenerateShoppingCarts =
                _validation.CanGenerateShoppingCarts();

            ViewBag.CanGenerateCartItems =
                _validation.CanGenerateCartItems();

            ViewBag.CanGenerateInventory =
                _validation.CanGenerateInventory();

            ViewBag.CanGenerateOrders =
                _validation.CanGenerateOrders();

            ViewBag.CanGenerateOrderItems =
                _validation.CanGenerateOrderItems();

            ViewBag.CanGeneratePayments =
                _validation.CanGeneratePayments();

            ViewBag.CanGeneratePurchaseOrders =
                _validation.CanGeneratePurchaseOrders();

            ViewBag.CanGeneratePurchaseOrderItems =
                _validation.CanGeneratePurchaseOrderItems();

            ViewBag.CanGenerateReviews =
                _validation.CanGenerateReviews();

            ViewBag.CanGenerateBooks =
                _validation.CanGenerateBooks();

            ViewBag.CanGenerateBookAuthors =
                _validation.CanGenerateBookAuthors();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateAuthors()
        {
            return ExecuteGenerator(
                () => _authorsGenerator.Generate(),
                "Authors generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateCategories()
        {
            return ExecuteGenerator(
                () => _categoriesGenerator.Generate(),
                "Categories generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateGenres()
        {
            return ExecuteGenerator(
                () => _genresGenerator.Generate(),
                "Genres generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePublishers()
        {
            return ExecuteGenerator(
                () => _publishersGenerator.Generate(),
                "Publishers generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateSuppliers()
        {
            return ExecuteGenerator(
                () => _suppliersGenerator.Generate(),
                "Suppliers generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateRoles()
        {
            return ExecuteGenerator(
                () => _rolesGenerator.Generate(),
                "Roles generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateCustomers()
        {
            return ExecuteGenerator(
                () => _customersGenerator.Generate(),
                "Customers generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateEmployees()
        {
            return ExecuteGenerator(
                () => _employeesGenerator.Generate(),
                "Employees generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateAddresses()
        {
            return ExecuteGenerator(
                () => _addressesGenerator.Generate(),
                "Addresses generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateShoppingCarts()
        {
            return ExecuteGenerator(
                () => _shoppingCartsGenerator.Generate(),
                "ShoppingCarts generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateCartItems()
        {
            return ExecuteGenerator(
                () => _cartItemsGenerator.Generate(),
                "CartItems generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateInventory()
        {
            return ExecuteGenerator(
                () => _inventoryGenerator.Generate(),
                "Inventory generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateOrders()
        {
            return ExecuteGenerator(
                () => _ordersGenerator.Generate(),
                "Orders generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateOrderItems()
        {
            return ExecuteGenerator(
                () => _orderItemsGenerator.Generate(),
                "OrderItems generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePayments()
        {
            return ExecuteGenerator(
                () => _paymentsGenerator.Generate(),
                "Payments generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePurchaseOrders()
        {
            return ExecuteGenerator(
                () => _purchaseOrdersGenerator.Generate(),
                "PurchaseOrders generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePurchaseOrderItems()
        {
            return ExecuteGenerator(
                () => _purchaseOrderItemsGenerator.Generate(),
                "PurchaseOrderItems generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateReviews()
        {
            return ExecuteGenerator(
                () => _reviewsGenerator.Generate(),
                "Reviews generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateBooks()
        {
            return ExecuteGenerator(
                () => _booksGenerator.Generate(),
                "Books generated successfully."
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateBookAuthors()
        {
            return ExecuteGenerator(
                () => _bookAuthorsGenerator.Generate(),
                "BookAuthors generated successfully."
            );
        }

        private ActionResult ExecuteGenerator(Action generate, string successMessage)
        {
            try
            {
                generate();
                TempData["Success"] = successMessage;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}