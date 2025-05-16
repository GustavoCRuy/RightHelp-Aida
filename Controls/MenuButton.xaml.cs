using System.Windows;
using System.Windows.Controls;

namespace RightHelp___Aida.Controls {
    /// <summary>
    /// Interação lógica para MenuButton.xam
    /// </summary>
    public partial class MenuButton : UserControl
    {
        public event EventHandler MenuClicked;

        public MenuButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
