using System.Windows;
using System.Windows.Controls;
using RightHelp___Aida.Services;

namespace RightHelp___Aida.Views
{
    public partial class Registro : Page
    {
        public Registro()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login
            {
                FirstName = FirstNameBox.Text,
                Email = EmailBox.Text,
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
            };

            bool registrado = await login.RegisterAsync();
            if (registrado)
            {
                MessageBox.Show("Registro realizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new Registro_Login());
            }
            else
            {
                MessageBox.Show("Erro ao registrar. Verifique os dados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registro_Login());
        }
    }
}
