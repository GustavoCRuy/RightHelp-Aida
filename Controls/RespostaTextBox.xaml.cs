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

namespace RightHelp___Aida.Controls
{
    public partial class RespostaTextBox : UserControl
    {
        public RespostaTextBox()
        {
            InitializeComponent();
        }

        // Método para adicionar texto na TextBox de forma thread-safe e auto-scroll
        public void AppendText(string text)
        {
            // Executa no thread da UI para evitar problemas com acesso a controles
           
                RespostaTextBoxControl.AppendText(text);
                RespostaTextBoxControl.ScrollToEnd();
            
        }
        public void ClearText()
        {
            Dispatcher.Invoke(() =>
            {
                RespostaTextBoxControl.Clear();
            });
        }
    }
}

