using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;
using HardwareStore.DB;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;

namespace HardwareStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для InventoryPage.xaml
    /// </summary>
    public partial class InventoryPage : Page
    {
        private List<string> lowStockProducts = new List<string>();


        public InventoryPage()
        {
            InitializeComponent();
            LoadInventoryData();
            SetUserPermissions();
            this.DataContext = this;
        }

        private void LoadInventoryData()
        {
            try
            {
                var inventoryList = (from product in App.db.Products
                                     join productType in App.db.ProductTypes on product.ProductTypeID equals productType.ProductTypeID
                                     join supplier in App.db.Suppliers on product.SupplierID equals supplier.SupplierID
                                     join reserv in App.db.reservation_status on product.id_reserv equals reserv.id_reserv
                                     join warehouse in App.db.Warehouse on product.id_warehouse equals warehouse.id_warehouse
                                     join orderStatus in App.db.OrderStatus on product.id_status_order equals orderStatus.id_status
                                     select new
                                     {
                                         product.ProductID,
                                         product.Name,            // Название продукта
                                         product.order_date,      // Дата заказа
                                         product.delivery_date,  // Добавлен новый столбец
                                         productType.TypeName,    // Тип продукта
                                         product.PurchasePrice,   // Цена закупки
                                         product.SellingPrice,    // Цена продажи
                                         product.StorageConditions, // Условия хранения
                                         product.Quantity,        // Количество
                                         supplier.SupplierName,   // Поставщик
                                         reserv.name_reserv,      // Статус резервирования
                                         warehouse.warehouse_name, // Название склада
                                         orderStatus.status_name  // Статус заказа
                                     }).ToList();

                InventoryDataGrid.ItemsSource = null;
                InventoryDataGrid.ItemsSource = inventoryList;

                lowStockProducts.Clear();
                foreach (var product in inventoryList)
                {
                    if (product.Quantity.HasValue && product.Quantity < 5)
                    {
                        lowStockProducts.Add(product.Name);
                    }
                }

                UpdateNotificationBell();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateNotificationBell()
        {
            NotificationBell.Content = $"🔔 ({lowStockProducts.Count})";
        }

        private void NotificationBell_Click(object sender, RoutedEventArgs e)
        {
            if (lowStockProducts.Count > 0)
            {
                MessageBox.Show($"Низкий уровень запасов у следующих товаров:\n{string.Join("\n", lowStockProducts)}",
                    "Оповещение о запасах", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Все запасы в норме.", "Оповещение о запасах", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetUserPermissions()
        {
            var currentUser = App.db.Users.FirstOrDefault(u => u.Username == "currentUser");

            if (currentUser == null)
                return;

            var role = App.db.Roles.FirstOrDefault(r => r.RoleID == currentUser.RoleID);
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addProductPage = new AddProductPage(LoadInventoryData);
            NavigationService.Navigate(addProductPage);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void OtchetProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    Title = "Сохранить отчет"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    Document doc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    doc.Open();

                    BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font titleFont = new Font(baseFont, 16, Font.BOLD);
                    Font headerFont = new Font(baseFont, 12, Font.BOLD);
                    Font dataFont = new Font(baseFont, 10, Font.NORMAL);

                    PdfContentByte cb = writer.DirectContent;
                    cb.BeginText();
                    cb.SetFontAndSize(baseFont, 13);
                    cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Отчёт по складу", 300f, doc.PageSize.Height - 30f, 0);
                    cb.EndText();

                    doc.Add(new iTextSharp.text.Paragraph("\n"));
                    doc.Add(new iTextSharp.text.Paragraph($"Дата: {DateTime.Now:dd.MM.yyyy}\n\n", dataFont));


                    PdfPTable table = new PdfPTable(5)
                    {
                        WidthPercentage = 100
                    };

                    string[] headers = { "Название", "Дата заказа", "Тип", "Цена закупки", "Количество" };
                    foreach (string header in headers)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont))
                        {
                            BackgroundColor = new BaseColor(211, 211, 211),
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 5
                        };
                        table.AddCell(headerCell);
                    }

                    var inventoryList = (from product in App.db.Products
                                         join productType in App.db.ProductTypes on product.ProductTypeID equals productType.ProductTypeID
                                         select new
                                         {
                                             product.Name,
                                             product.order_date,
                                             TypeName = productType.TypeName,
                                             product.PurchasePrice,
                                             product.Quantity
                                         }).ToList();

                    foreach (var product in inventoryList)
                    {
                        table.AddCell(new PdfPCell(new Phrase(product.Name ?? "—", dataFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(product.order_date?.ToString("dd.MM.yyyy") ?? "—", dataFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(product.TypeName ?? "—", dataFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(product.PurchasePrice.ToString("F2"), dataFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(product.Quantity?.ToString() ?? "0", dataFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("Отчёт успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}