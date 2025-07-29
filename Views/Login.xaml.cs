using System.Windows;
using System.Windows.Controls;
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
            var login = new User
            {
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
            };

            bool autenticado = await login.AuthenticateAsync();
            if (autenticado)
            {
                var mainWindow = new MainWindow();
                // mainWindow.Show();

                // Window.GetWindow(this)?.Close();
                NavigationService?.Navigate(mainWindow);
            }
            else
            {
                MessageBox.Show("Usuário ou senha incorretos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registro());
        }
    }
}