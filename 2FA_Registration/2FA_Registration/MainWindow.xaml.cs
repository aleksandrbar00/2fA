using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.IO;
using System.Security.Cryptography;

namespace _2FA_Registration
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RegistrationButton.IsEnabled = false;
        }

        List<string> serial_numbers = new List<string>();

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            serial_numbers.Clear();

            //Получение списка USB-накопителей
            foreach (System.Management.ManagementObject drive in new System.Management.ManagementObjectSearcher
                ("select * from Win32_USBHub where Caption='Запоминающее устройство для USB'").Get())
            {
                //Получение cерийного номера
                string[] splitDeviceId = drive["PNPDeviceID"].ToString().Trim().Split('\\');
                serial_numbers.Add(splitDeviceId[2].Trim());
            }

            switch(serial_numbers.Count)
            {
                case 1:
                    MessageBox.Show("Проверка успешно пройдена!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    CheckButton.IsEnabled = false;
                    RegistrationButton.IsEnabled = true;
                    break;
                case 0:
                    MessageBox.Show("USB-накопитель не обнаружен!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                default:
                    MessageBox.Show("К ПК подключено несколько USB-накопителей!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string onepwd = OnePasswordBox.Password;
            string twopwd = TwoPasswordBox.Password;

            if(login.Length != 0 && onepwd == twopwd)
            {   
                if(CheckLogin(login))
                {
                    string hash_login = GetHash(login);
                    string hash_pwd = GetHash(onepwd + hash_login);
                    string hash_sn = GetHash(serial_numbers[0] + hash_login);
                    string data_auth = login + ":" + hash_pwd + ":" + hash_sn + ":";

                    //Запись в файл
                    using (FileStream fstream = new FileStream("database", FileMode.Append))
                    {
                        byte[] array = System.Text.Encoding.Default.GetBytes(data_auth);
                        fstream.Write(array, 0, array.Length);
                    }

                    MessageBox.Show("Регистрация успешно пройдена!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoginTextBox.Clear();
                    OnePasswordBox.Clear();
                    TwoPasswordBox.Clear();
                    CheckButton.IsEnabled = true;
                    RegistrationButton.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Выбранный логин используется!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                if(login.Length == 0)
                    MessageBox.Show("Введите логин!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Пароли не совпадают!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetHash(string input)
        {
            var sha1 = SHA1Cng.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        public bool CheckLogin(string login)
        {   
            bool response = true;

            if (File.Exists("database"))
            {
                string auth_data = null;

                //Чтение из файла
                using (FileStream fstream = File.OpenRead("database"))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    auth_data = System.Text.Encoding.Default.GetString(array);
                }

                List<string> logins = new List<string>();
                string[] auth_data_array = auth_data.Split(':');

                for (int i = 0; i < auth_data_array.Count() - 1;)
                {
                    logins.Add(auth_data_array[i]);
                    i += 3;
                }

                foreach (var tmp in logins)
                {
                    if (tmp == login)
                    {
                        response = false;
                        break;
                    }
                    else
                        response = true;
                }
            }

            return response;
        }
    }
}
