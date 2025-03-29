using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HardwareStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для SupplierInventoryPage.xaml
    /// </summary>
    public partial class SupplierInventoryPage : Page
    {

        public SupplierInventoryPage()
        {
            InitializeComponent();
            LoadSupplierInventory();
        }
        private void LoadSupplierInventory()
        {
            try
            {
                var inventoryDiscrepancies = (from product in App.db.Products
                                              join supplierStock in App.db.SupplierStocks on product.ProductID equals supplierStock.ProductID
                                              join managerStock in App.db.ManagerStocks on product.ProductID equals managerStock.ProductID
                                              select new Product
                                              {
                                                  ProductID = product.ProductID,
                                                  Name = product.Name,
                                                  ManagerQuantity = managerStock.Quantity,
                                                  SupplierQuantity = supplierStock.Quantity,
                                                  DiscrepancyAmount = supplierStock.Quantity - managerStock.Quantity
                                              }).ToList();


                // Проверка на нехватку товара
                var lowStockProducts = inventoryDiscrepancies.Where(p => p.DiscrepancyAmount < 0).Select(p => p.Name).ToList();
                SupplierInventoryDataGrid.ItemsSource = inventoryDiscrepancies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ReplenishStock_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierInventoryDataGrid.SelectedItem is null) return;

            var selectedProduct = (Product)SupplierInventoryDataGrid.SelectedItem;

            if (selectedProduct.DiscrepancyAmount <= 0)
            {
                MessageBox.Show("Расхождений нет, пополнение не требуется.");
                return;
            }

            try
            {
                var managerStock = App.db.ManagerStocks.FirstOrDefault(m => m.ProductID == selectedProduct.ProductID);
                if (managerStock != null)
                {
                    managerStock.Quantity += selectedProduct.DiscrepancyAmount;
                    App.db.SaveChanges();
                }

                MessageBox.Show("Товар успешно пополнен!");
                LoadSupplierInventory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при пополнении товара: {ex.Message}");
            }
        }

        public class Product
        {
            public int ProductID { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public int DiscrepancyAmount { get; set; }
            public int SupplierQuantity { get; set; }
            public int ManagerQuantity { get; set; }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}