using PAP.DataAccess;
using PAP.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Dictionary<string, int> rows = GetRowCounts();

            var tables = new List<AdminTableStatus>
            {
                Status("Authors",            0, _validation.CanGenerateAuthors(),            rows),
                Status("Categories",         0, _validation.CanGenerateCategories(),         rows),
                Status("Genres",             0, _validation.CanGenerateGenres(),             rows),
                Status("Publishers",         0, _validation.CanGeneratePublishers(),         rows),
                Status("Suppliers",          0, _validation.CanGenerateSuppliers(),          rows),
                Status("Roles",              0, _validation.CanGenerateRoles(),              rows),
                Status("Customers",          0, _validation.CanGenerateCustomers(),          rows),
                Status("Books",              1, _validation.CanGenerateBooks(),              rows, "Publishers", "Categories", "Genres"),
                Status("Employees",          1, _validation.CanGenerateEmployees(),          rows, "Roles"),
                Status("Addresses",          1, _validation.CanGenerateAddresses(),          rows, "Customers"),
                Status("ShoppingCarts",      1, _validation.CanGenerateShoppingCarts(),      rows, "Customers"),
                Status("PurchaseOrders",     1, _validation.CanGeneratePurchaseOrders(),     rows, "Suppliers"),
                Status("BookAuthors",        2, _validation.CanGenerateBookAuthors(),        rows, "Books", "Authors"),
                Status("Inventory",          2, _validation.CanGenerateInventory(),          rows, "Books"),
                Status("Reviews",            2, _validation.CanGenerateReviews(),            rows, "Books", "Customers"),
                Status("Orders",             2, _validation.CanGenerateOrders(),             rows, "Customers", "Addresses"),
                Status("CartItems",          2, _validation.CanGenerateCartItems(),          rows, "ShoppingCarts", "Books"),
                Status("PurchaseOrderItems", 2, _validation.CanGeneratePurchaseOrderItems(), rows, "PurchaseOrders", "Books"),
                Status("OrderItems",         3, _validation.CanGenerateOrderItems(),         rows, "Orders", "Books"),
                Status("Payments",           3, _validation.CanGeneratePayments(),           rows, "Orders")
            };

            return View(tables);
        }

        /// <summary>
        /// Estado de una tabla para la pantalla de Admin.
        /// </summary>
        public class AdminTableStatus
        {
            public string Name { get; set; }
            public int Level { get; set; }
            public int Rows { get; set; }
            public bool CanGenerate { get; set; }
            public List<string> Missing { get; set; }
            public string Action { get { return "Generate" + Name; } }
            public bool HasData { get { return Rows > 0; } }
        }

        private static AdminTableStatus Status(string name, int level, bool canGenerate,
            Dictionary<string, int> rows, params string[] dependencies)
        {
            int count;
            rows.TryGetValue(name, out count);

            return new AdminTableStatus
            {
                Name = name,
                Level = level,
                Rows = count,
                CanGenerate = canGenerate,
                Missing = dependencies
                    .Where(d => !rows.ContainsKey(d) || rows[d] == 0)
                    .ToList()
            };
        }

        public class TableRowCount
        {
            public string Name { get; set; }
            public int Total { get; set; }
        }

        /// <summary>
        /// Cuenta los registros de las 20 tablas en una sola consulta.
        /// </summary>
        private static Dictionary<string, int> GetRowCounts()
        {
            string[] names =
            {
                "Authors", "Categories", "Genres", "Publishers", "Suppliers", "Roles", "Customers",
                "Books", "Employees", "Addresses", "ShoppingCarts", "PurchaseOrders",
                "BookAuthors", "Inventory", "Reviews", "Orders", "CartItems", "PurchaseOrderItems",
                "OrderItems", "Payments"
            };

            string sql = string.Join(" UNION ALL ",
                names.Select(n => "SELECT '" + n + "' AS Name, COUNT(*) AS Total FROM dbo.[" + n + "]"));

            try
            {
                using (var db = new BookStoreDBEntities())
                {
                    return db.Database.SqlQuery<TableRowCount>(sql)
                        .ToList()
                        .ToDictionary(r => r.Name, r => r.Total);
                }
            }
            catch
            {
                return new Dictionary<string, int>();
            }
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