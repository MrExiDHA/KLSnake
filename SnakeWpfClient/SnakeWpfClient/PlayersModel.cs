using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SnakeWpfClient
{
    class PlayersModel
    {
        // Переменная имени игрока и её обёртка
        private string _player;
        public string Player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Player)));
            }
        }

        // EventHandler для поддержки и реализации ивентов
        public event PropertyChangedEventHandler PropertyChanged = (s, a) => { };
    }
}
