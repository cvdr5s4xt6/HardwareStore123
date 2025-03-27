using HardwareStore.DB;
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
    /// Логика взаимодействия для AddProductPage.xaml
    /// </summary>
    public partial class AddProductPage : Page
    {
        private Action updateInventoryCallback;
        public int SelectedProductTypeID { get; set; }
        public int SelectedProductWID { get; set; }

        public int SelectedReservationStatusID { get; set; }
        public int SelectedOrderStatusID { get; set; }

        public AddProductPage(Action updateInventory)
        {
            InitializeComponent();
            updateInventoryCallback = updateInventory;
            DataContext = this;
            LoadSuppliers();
            LoadProductTypes();
            LoadWarehouses(); 
            LoadReservationStatuses(); 
            LoadOrderStatuses(); 
        }

        private void LoadReservationStatuses()
        {
            var reservationStatus = App.db.reservation_status.FirstOrDefault(r => r.id_reserv == 2);
            if (reservationStatus != null)
            {
                ReservationStatusComboBox.ItemsSource = new List<reservation_status> { reservationStatus };
                ReservationStatusComboBox.DisplayMemberPath = "name_reserv";
                ReservationStatusComboBox.SelectedValuePath = "id_reserv";
                ReservationStatusComboBox.SelectedValue = reservationStatus.id_reserv;
            }
        }

        private void LoadOrderStatuses()
        {
            var orderStatus = App.db.OrderStatus.FirstOrDefault(o => o.id_status == 1);
            if (orderStatus != null)
            {
                OrderStatusComboBox.ItemsSource = new List<OrderStatus> { orderStatus };
                OrderStatusComboBox.DisplayMemberPath = "status_name";
                OrderStatusComboBox.SelectedValuePath = "id_status";
                OrderStatusComboBox.SelectedValue = orderStatus.id_status;
            }
        }

        private void LoadProductTypes()
        {
            var productTypes = App.db.ProductTypes.ToList();
            ProductTypeComboBox.ItemsSource = productTypes;
            ProductTypeComboBox.DisplayMemberPath = "TypeName"; 
            ProductTypeComboBox.SelectedValuePath = "ProductTypeID";   
        }

        private void LoadSuppliers()
        {
            var suppliers = App.db.Suppliers.ToList();
            SupplierComboBox.ItemsSource = suppliers;
            SupplierComboBox.DisplayMemberPath = "SupplierName";
            SupplierComboBox.SelectedValuePath = "SupplierID";
        }

        private void LoadWarehouses()
        {
            var warehouses = App.db.Warehouse.ToList();
            WarehouseComboBox.ItemsSource = warehouses;
            WarehouseComboBox.DisplayMemberPath = "warehouse_name";
            WarehouseComboBox.SelectedValuePath = "id_warehouse";
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var productName = NameTextBox.Text.Trim();
                var warehouseID = SelectedProductWID;

                var productTypeID = SelectedProductTypeID;
                var purchasePrice = decimal.Parse(PurchasePriceTextBox.Text); // Получаем цену закупки
                var sellingPrice = decimal.Parse(SellingPriceTextBox.Text); // Получаем цену продажи
                var storageConditions = StorageConditionsTextBox.Text;
                var supplierID = (int)SupplierComboBox.SelectedValue;
                var quantity = int.Parse(QuantityTextBox.Text);

                var reservationStatusID = SelectedReservationStatusID;
                var orderStatusID = SelectedOrderStatusID;

                var product = new Products
                {
                    Name = productName,
                    ProductTypeID = productTypeID,
                    PurchasePrice = purchasePrice,
                    SellingPrice = sellingPrice,
                    StorageConditions = storageConditions,
                    SupplierID = supplierID,
                    Quantity = quantity,
                    id_status_order = 1,
                    id_reserv = reservationStatusID,
                    id_warehouse = warehouseID
                };

                App.db.Products.Add(product);
                App.db.SaveChanges();
                MessageBox.Show("Товар добавлен успешно");
                Dispatcher.Invoke(() => updateInventoryCallback?.Invoke());

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}