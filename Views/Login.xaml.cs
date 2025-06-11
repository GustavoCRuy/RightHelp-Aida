using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;
using MySqlConnector;
using RightHelp___Aida.Services;
using RightHelp___Aida.Services.Constants;

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
                // Buscar o UserId do usuário autenticado
                using var connection = new MySqlConnection(Const.connectionString);
                await connection.OpenAsync();
                var cmd = new MySqlCommand("SELECT user_id FROM usuarios WHERE username = @username", connection);
                cmd.Parameters.AddWithValue("@username", login.Username);

                var result = await cmd.ExecuteScalarAsync();
                UserSession.UserId = result?.ToString();

                var mainWindow = new MainWindow();
                mainWindow.Show();

                Window.GetWindow(this)?.Close();
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