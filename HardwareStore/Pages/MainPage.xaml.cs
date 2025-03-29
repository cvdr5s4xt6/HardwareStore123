using HardwareStore.DB;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private int userRoleID;
        public MainPage(int roleID)
        {
            InitializeComponent();
            userRoleID = roleID;
            SetMenuByRole(userRoleID);
        }
        private void SetMenuByRole(int roleID)
        {
            switch (roleID)
            {
                case 1: // Роль менеджера
                    ShowManagerMenu();
                    break;
                case 2: // Роль поставщика
                    ShowWarehouseMenu();
                    break;
                case 3: // Роль клиента
                    ShowClientMenu();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя.");
                    break;
            }
        }

        private void ShowManagerMenu()
        {
            Manager.Visibility = Visibility.Visible;

            Client.Visibility = Visibility.Collapsed;
            ClientMenu.Visibility = Visibility.Collapsed;

            Warehouse.Visibility = Visibility.Collapsed;
            ManagerMenu.Visibility = Visibility.Visible;
            WarehouseMenu.Visibility = Visibility.Collapsed;
        }

        private void ShowWarehouseMenu()
        {
            Warehouse.Visibility = Visibility.Visible;


            Client.Visibility = Visibility.Collapsed;
            ClientMenu.Visibility = Visibility.Collapsed;

            Manager.Visibility = Visibility.Collapsed;
            ManagerMenu.Visibility = Visibility.Collapsed;
            WarehouseMenu.Visibility = Visibility.Visible;
        }
        private void ShowClientMenu()
        {
            Client.Visibility = Visibility.Visible;
            ClientMenu.Visibility = Visibility.Visible;

            Manager.Visibility = Visibility.Collapsed;
            Warehouse.Visibility = Visibility.Collapsed;
            ManagerMenu.Visibility = Visibility.Collapsed;
            WarehouseMenu.Visibility = Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new InventoryPage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WarehousePage());
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SupplierInventoryPage());
        }
    }
}