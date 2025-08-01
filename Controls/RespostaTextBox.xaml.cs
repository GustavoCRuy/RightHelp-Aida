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

        public async Task AppendFormattedText(string textoDaIa)
        {
            var regex = new Regex(@"\*\*(.*?)\*\*");
            var matches = regex.Matches(textoDaIa);

            int lastIndex = 0;
            var p = new Paragraph();

            if (matches.Count == 0)
            {
                p.Inlines.Add(new Run(textoDaIa));
                RespostaTextBoxControl.Document.Blocks.Add(p);
                RespostaTextBoxControl.ScrollToEnd();
                return;
            }

            foreach (Match match in matches)
            {
                string textoAnterior = textoDaIa.Substring(lastIndex, match.Index - lastIndex);
                await AppendTypingEffect(p, textoAnterior);

                string textoNegrito = match.Groups[1].Value;
                var runNegrito = new Run(textoNegrito) { FontWeight = FontWeights.Bold };
                p.Inlines.Add(runNegrito);
                await Task.Delay(50);

                lastIndex = match.Index + match.Length;
            }

            string textoFinal = textoDaIa.Substring(lastIndex);
            await AppendTypingEffect(p, textoFinal);

            RespostaTextBoxControl.Document.Blocks.Add(p);
            RespostaTextBoxControl.ScrollToEnd();
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