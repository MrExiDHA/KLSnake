using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;

namespace SnakeWpfClient
{
    class SnakeModel : INotifyPropertyChanged
    {
        // Переменная цвета ячейки поля и обёртка для неё
        private Brush _back;
        public Brush Back
        {
            get
            {
                return _back;
            }
            set
            {
                _back = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Back)));
            }
        }

        // EventHandler для поддержки и реализации ивентов
        public event PropertyChangedEventHandler PropertyChanged = (s, a) => { };
    }
}
