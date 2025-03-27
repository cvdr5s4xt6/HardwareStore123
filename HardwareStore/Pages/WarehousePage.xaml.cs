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
    /// Логика взаимодействия для WarehousePage.xaml
    /// </summary>
    public partial class WarehousePage : Page
    {
        public WarehousePage()
        {
            InitializeComponent();
            LoadPendingOrders();
        }

        private void LoadPendingOrders()
        {
            try
            {
                var pendingOrders = (from product in App.db.Products
                                     join orderStatus in App.db.OrderStatus on product.id_status_order equals orderStatus.id_status
                                     where orderStatus.status_name == "Заказ отправлен"
                                     select new OrderViewModel
                                     {
                                         ProductID = product.ProductID,
                                         Name = product.Name,
                                         Quantity = product.Quantity ?? 0,
                                         OrderDate = product.order_date.HasValue ? product.order_date.Value : (DateTime?)null,
                                         DeliveryDate = product.delivery_date.HasValue ? product.delivery_date.Value : (DateTime?)null,
                                         Status = orderStatus.status_name.Trim()
                                     }).ToList();
                WarehouseDataGrid.ItemsSource = null;
                WarehouseDataGrid.ItemsSource = pendingOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void SendProduct_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseDataGrid.SelectedItem is OrderViewModel selectedOrder)
            {
                var product = App.db.Products.FirstOrDefault(p => p.ProductID == selectedOrder.ProductID);
                if (product != null)
                {
                    product.id_status_order = 3;  // Устанавливаем значение id_status для "Доставлен"
                    product.delivery_date = DateTime.Now; 

                    App.db.SaveChanges();

                    MessageBox.Show("Товар отправлен!");
                    LoadPendingOrders();
                }
                else
                {
                    MessageBox.Show("Товар не найден в базе данных.");
                }
            }
        }

        private void DelayProduct_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseDataGrid.SelectedItem is OrderViewModel selectedOrder)
            {
                DateTime? newDeliveryDate = GetNewDeliveryDate();
                if (newDeliveryDate.HasValue)
                {
                    var product = App.db.Products.FirstOrDefault(p => p.ProductID == selectedOrder.ProductID);
                    if (product != null)
                    {
                        product.delivery_date = newDeliveryDate.Value;
                        App.db.SaveChanges();

                        MessageBox.Show("Дата доставки обновлена.");
                        LoadPendingOrders();
                    }
                }
            }
        }

        private DateTime? GetNewDeliveryDate()
        {
            if (DeliveryDatePicker.SelectedDate.HasValue && TimeSpan.TryParse(DeliveryTimeBox.Text, out TimeSpan time))
            {
                return DeliveryDatePicker.SelectedDate.Value.Date + time;
            }
            MessageBox.Show("Введите корректную дату и время.");
            return null;
        }

        private void ConfirmDelay_Click(object sender, RoutedEventArgs e)
        {
            DelayProduct_Click(sender, e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public class OrderViewModel
        {
            public int ProductID { get; set; }
            public string Name { get; set; }
            public string TypeName { get; set; }
            public int Quantity { get; set; }
            public DateTime? OrderDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public string Status { get; set; }
        }
    }
}
