using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RightHelp___Aida.Enums;
using RightHelp___Aida.Services.Helpers;

namespace RightHelp___Aida.ViewModels
{
    public class AidaViewModel : INotifyPropertyChanged
    {
        private AidaModoInteracao modoAtual = AidaModoInteracao.ModoEscrita;

        public AidaModoInteracao ModoAtual
        {
            get => modoAtual;
            set
            {
                if (modoAtual != value)
                {
                    modoAtual = value;
                    OnPropertyChanged(nameof(ModoAtual));
                }
            }
        }
        public ICommand AlternarModoCommand { get; }
        public AidaViewModel()
        {
            AlternarModoCommand = new RelayCommand(AlternarModo);
        }
        private void AlternarModo()
        {
            ModoAtual = ModoAtual == AidaModoInteracao.ModoLeitura
                ? AidaModoInteracao.ModoEscrita
                : AidaModoInteracao.ModoLeitura;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string nome) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
    }
}
