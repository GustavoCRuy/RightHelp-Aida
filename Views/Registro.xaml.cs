using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            RegisterButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var user = new User
                {
                    FirstName = FirstNameBox.Text,
                    Email = EmailBox.Text,
                    Username = UsernameBox.Text,
                    Password = PasswordBox.Password
                };

                string error;
                if (!user.IsValid(out error))
                {
                    MessageBox.Show(error, "Erro de validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool registrado = await user.RegisterAsync();
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
            finally
            {
                Mouse.OverrideCursor = null;
                RegisterButton.IsEnabled = true;
            }
        }

        private void LoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registro_Login());
        }

        private void FirstNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
