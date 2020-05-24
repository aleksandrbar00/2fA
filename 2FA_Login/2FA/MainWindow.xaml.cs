using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace _2FA
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<AuthData> list_auth_data = new List<AuthData>();

        public MainWindow()
        {
            InitializeComponent();

            string auth_data = null;

            //Чтение из файла
            using (FileStream fstream = File.OpenRead("database"))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                auth_data = System.Text.Encoding.Default.GetString(array);
            }

            string[] auth_data_array = auth_data.Split(':');

            for (int i = 0; i < auth_data_array.Count() - 1; )
            {
                list_auth_data.Add(new AuthData(auth_data_array[i], auth_data_array[i+1], auth_data_array[i+2]));
                i += 3;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string pwd = PasswordBox.Password;
            string hash_login = GetHash(login);
            string hash_pwd = GetHash(pwd + hash_login);

            List<string> serial_numbers = new List<string>();

            //Получение списка USB-накопителей
            foreach (System.Management.ManagementObject drive in new System.Management.ManagementObjectSearcher
                ("select * from Win32_USBHub where Caption='Запоминающее устройство для USB'").Get())
            {
                //Получение cерийного номера
                string[] splitDeviceId = drive["PNPDeviceID"].ToString().Trim().Split('\\');
                serial_numbers.Add(splitDeviceId[2].Trim());
            }

            bool if_find = false;

            foreach (var tmp in list_auth_data)
            {
                if (tmp.login == login)
                {
                    Console.WriteLine("L");

                    if (tmp.pwd == hash_pwd)
                    {
                        Console.WriteLine("P");

                        foreach (var sn in serial_numbers)
                        {
                            Console.WriteLine(tmp.sn + " & " + GetHash(sn + hash_login));

                            if (tmp.sn == GetHash(sn + hash_login))
                            {
                                Console.WriteLine("S");
                                if_find = true;
                                break;
                            }
                        }
                    }
                }
            }

            if(if_find)
                MessageBox.Show("Двухфакторная аутентификация успешно пройдена!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Двухфакторная аутентификация не пройдена!\nПроверьте логин и пароль, а также подключение USB-ключа", "",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        public string GetHash(string input)
        {
            var sha1 = SHA1Cng.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

    }
}
