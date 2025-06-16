using System.Windows;
using System.Windows.Controls;
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
            try
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
                    var cmd = new MySqlCommand("SELECT user_id FROM users WHERE username = @username", connection);
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
            catch (MySqlException ex)
            {
                MessageBox.Show($"Erro de banco de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Registro());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao navegar para o registro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}