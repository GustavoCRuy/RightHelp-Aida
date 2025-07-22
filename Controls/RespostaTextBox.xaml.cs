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
       public void AppendParagraph(string text, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph(new Run(text))
                {
                    Foreground = color,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                RespostaTextBoxControl.Document.Blocks.Add(paragraph);
                RespostaTextBoxControl.ScrollToEnd();
            });
        }

        public void AppendTextToLastParagraph(string text)
        {
            Dispatcher.Invoke(() =>
            {
                if (RespostaTextBoxControl.Document.Blocks.LastBlock is Paragraph lastParagraph)
                {
                    lastParagraph.Inlines.Add(new Run(text));
                    RespostaTextBoxControl.ScrollToEnd();
                }
            });
        }

    }
}

