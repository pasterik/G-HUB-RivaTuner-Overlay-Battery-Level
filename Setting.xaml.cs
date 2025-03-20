using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GHUB_Overlay
{
    public partial class Setting : Window
    {
        private MenuItem selectedItem;

        public Setting()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize; // Фіксуємо розмір вікна
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // Відкриваємо по центру екрана
        }

        private void OnDeviceButtonClick(object sender, RoutedEventArgs e)
        {
            // Створення контекстного меню з чекбоксами
            ContextMenu contextMenu = new ContextMenu();
            List<string> devices = new List<string>
            {
                "Миша Logitech G502",
                "Клавіатура Corsair K95",
                "Гарнітура HyperX Cloud II",
                "Геймпад Xbox Series X",
                "Монітор ASUS ROG Strix"
            };

            foreach (var device in devices)
            {
                MenuItem menuItem = new MenuItem
                {
                    Header = device
                };

                // Додаємо чекбокс для кожного елемента
                CheckBox checkBox = new CheckBox
                {
                    Content = device,
                    IsChecked = false // За замовчуванням чекбокс не вибраний
                };
                checkBox.Checked += (s, args) => OnDeviceChecked(menuItem);
                checkBox.Unchecked += (s, args) => OnDeviceUnchecked(menuItem);

                menuItem.Icon = checkBox;
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        private void OnDeviceChecked(MenuItem menuItem)
        {
            // Якщо вибрано новий пристрій, скидаємо вибір з попереднього
            if (selectedItem != null)
            {
                selectedItem.IsChecked = false;
            }

            // Позначаємо вибраний пристрій
            menuItem.IsChecked = true;
            selectedItem = menuItem;
        }

        private void OnDeviceUnchecked(MenuItem menuItem)
        {
            // Деактивуємо вибір, якщо чекбокс знятий
            menuItem.IsChecked = false;
            selectedItem = null;
        }
    }
}
