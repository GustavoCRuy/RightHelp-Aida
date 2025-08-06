using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace RightHelp___Aida.Controls
{
    public partial class RespostaTextBox : UserControl
    {
        public RespostaTextBox()
        {
            InitializeComponent();
        }

        // Método para adicionar texto como um novo parágrafo
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

        // Método para adicionar texto parcial no último parágrafo (para o streaming)
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

        private async Task AppendTypingEffect(Paragraph paragraph, string text)
        {
            foreach (char c in text)
            {
                paragraph.Inlines.Add(new Run(c.ToString()));
                await Task.Delay(20);
            }
        }
    }
}