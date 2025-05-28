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
using Microsoft.Win32;
using RightHelp___Aida.Services;

namespace RightHelp___Aida.Views
{
    public partial class Registro_Login : Page
    {
        public Registro_Login()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login
            {
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
            };

            bool autenticado = await login.AuthenticateAsync();
            if (autenticado)
            {
                MessageBox.Show("Login bem-sucedido!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                // Navegar para a próxima página
            }
            else
            {
                MessageBox.Show("Usuário ou senha incorretos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Navegue para a página de registro
            NavigationService?.Navigate(new Registro());
        }
    }
}