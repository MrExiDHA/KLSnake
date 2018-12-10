using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;


namespace SnakeWpfClient
{
    class SnakeViewModel : INotifyPropertyChanged
    {
        // Объявление bind-переменных
        private int _rows;
        private int _columns;
        private int _gridWidth;
        private int _gridHeight;
        private int _score;
        private int _topScore;
        private int _round;
        private ObservableCollection<SnakeModel> _model;
        private ObservableCollection<PlayersModel> _players;
        
        // Объявления bind-комманд
        private CommandRealisation top;
        private CommandRealisation bottom;
        private CommandRealisation left;
        private CommandRealisation right;

        // Остальные переменные
        private SnakeClient _snake;
        private string _myname;
        private MapResponce _map;

        // Конструктор по-умолчанию
        // Инициализирует все нужные параметры и присваивает им значения по-умолчанию
        public SnakeViewModel()
        {
            _rows = 60;
            _columns = 50;
            _gridWidth = 10 * _columns;
            _gridHeight = 10 * _rows;
            _score = 0;
            _topScore = 0;
            _round = 0;
            _snake = new SnakeClient();
            _model = new ObservableCollection<SnakeModel>();
            _players = new ObservableCollection<PlayersModel>();
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    _model.Add(new SnakeModel() { Back = Brushes.White });
                }
            }
            // Запуск асинхронного потока обработки данных
            Task.Run(() => DoTheGame());
        }

        // Заполнение поля цветами в соответствии с map-ой полученной с сервера
        // Уверен, что это должно быть реализовано в модели, однако, сколько бы я не бился
        // и как не изощрялся, реализация этой функции в модели выкидывала исключение 
        // "Данный тип CollectionView не поддерживает изменения...",
        // когда View пыталась подгрузить нужные ей данные через привязку
        // Пытался через задание диспатчеру сделать, но тогда всё работало ОТНЮДЬ НЕ быстро,
        // да и на костыль больно было похоже (программа и так не без них, лишних не надо)
        // Слишком долго промучился с XAML и успел изучить MVVM паттерн только лишь на классических 
        // простых примерах, потому, наверное, не додумался как до ума довести всё это дело
        public void FullfillTheModel()
        {
            int wid = _map.GameBoardSize.Width;

            // Зачистка карты
            for (int y = 0; y < _map.GameBoardSize.Height; y++)
            {
                for (int x = 0; x < wid; x++)
                {
                    _model[x + wid * y].Back = Brushes.White;
                }
            }

            // Расстановка стен
            foreach (Wall W in _map.Walls)
                for (int y = W.Y; y < W.Y + W.Height; y++)
                    for (int x = W.X; x < W.X + W.Width; x++)
                        _model[x + wid * y].Back = Brushes.DarkBlue;

            // Расстановка игроков
            int tempTopScore = 0; // Переменная для подсчёта лидера раунда
            foreach (PlayerInfo p in _map.Players)
            {
                if (p.Snake != null)
                {
                    // Я или не Я
                    if (p.Name == _myname)
                    {
                        // Обновление счёта
                        if (_score != p.Snake.Count())
                            Score = p.Snake.Count();
                        // Запоминание наибольшей длины змейки
                        if (tempTopScore < _score)
                            tempTopScore = _score;
                        // Расстановка себя
                        foreach (Point cell in p.Snake)
                        {
                            if (p.IsSpawnProtected)
                                _model[cell.X + wid * cell.Y].Back = Brushes.LightGreen;
                            else
                                _model[cell.X + wid * cell.Y].Back = Brushes.Green;
                        }
                    }
                    else
                    {
                        // Запоминание наибольшей длины змейки
                        if (tempTopScore < p.Snake.Count())
                            tempTopScore = p.Snake.Count();
                        // Расстановка других игроков
                        foreach (Point cell in p.Snake)
                        {
                            if (p.IsSpawnProtected)
                                _model[cell.X + wid * cell.Y].Back = Brushes.Yellow;
                            else
                                _model[cell.X + wid * cell.Y].Back = Brushes.Orange;
                        }
                    }
                }
            }
            // Обновление лидера
            TopScore = tempTopScore;

            // Расстановка яблок
            foreach (Point ap in _map.Food)
            {
                _model[ap.X + wid * ap.Y].Back = Brushes.Red;
            }
        }

        //Асинхронная функция подгрузки карты и её изменения для отображения
        // Наверное, здесь тоже полно всего, что столо бы в модель вынести =\
        public async void DoTheGame()
        {
            // Получение собственного имени
            await Task.Run(() => _myname = _snake.GetNameAsync().Result);

            // Цикл выполняется пока игра IsStarted
            do
            {
                // Загрузка карты
                await Task.Run(() => _map = _snake.GetMapAsync().Result);
                // Проверка: не поменялись ли размеры карты
                // Если поменялись то пересобрать основу для карты
                // под нужные размеры и изменить bind-переменные
                if (_map.GameBoardSize.Width != _columns || _map.GameBoardSize.Height != _rows)
                {
                    _model = new ObservableCollection<SnakeModel>();
                    for (int y = 0; y < _map.GameBoardSize.Height; y++)
                    {
                        for (int x = 0; x < _map.GameBoardSize.Width; x++)
                        {
                            _model.Add(new SnakeModel() { Back = Brushes.White });
                        }
                    }
                    Rows = _map.GameBoardSize.Height;
                    Columns = _map.GameBoardSize.Width;
                    GridHeight = 10 * _rows;
                    GridWidth = 10 * _columns;
                }

                // Если сменился раунд то обновляем цифру раунда
                if (_round != _map.RoundNumber)
                {
                    Round = _map.RoundNumber;
                }

                // Вызов функции обновления карты и райз ивента об обновлении карты
                FullfillTheModel();
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ColorModel)));

                // Пересоздание bind-списка игроков и райз ивента об этом
                // Это делается, Господь, при каждом обновлении карты
                // Плохо, сам не хотел, но уже построенная кривая MVVM реализация 
                // вынудила делать так; либо я просто не замечаю другие варианты
                _players = new ObservableCollection<PlayersModel>();
                foreach (PlayerInfo p in _map.Players)
                {
                    if (p.Snake != null)
                    {
                        _players.Add(new PlayersModel() { Player = p.Name });
                    }
                }
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Players)));    
                
                // Ожидание конца хода
                await Task.Delay(_map.TimeUntilNextTurnMilliseconds);
            } while (_map.IsStarted == true);
        }

        // Реализация команд Вверх, Вниз, Влево, Вправо
        public CommandRealisation Top
        {
            get
            {
                return top ??
                    (top = new CommandRealisation(obj =>
                    {
                        _snake.SendDirectionAsync("Top");
                    }));
            }
        }
        public CommandRealisation Bottom
        {
            get
            {
                return bottom ??
                    (bottom = new CommandRealisation(obj =>
                    {
                        _snake.SendDirectionAsync("Bottom");
                    }));
            }
        }
        public CommandRealisation Left
        {
            get
            {
                return left ??
                    (left = new CommandRealisation(obj =>
                    {
                        _snake.SendDirectionAsync("Left");
                    }));
            }
        }
        public CommandRealisation Right
        {
            get
            {
                return right ??
                    (right = new CommandRealisation(obj =>
                    {
                        _snake.SendDirectionAsync("Right");
                    }));
            }
        }

        // Обёртка для bind-коллекции цветов для Border-ов
        public ObservableCollection<SnakeModel> ColorModel
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ColorModel)));
            }
        }

        // Обёртка для bind-коллекции игроков для StackPanel
        public ObservableCollection<PlayersModel> Players
        {
            get
            {
                return _players;
            }
            set
            {
                _players = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Players)));
            }
        }

        // Обёртки для bind-переменных, обозначающих:
        // Раунд(Round), Счёт лидера(TopScore), Мой счёт(Score), Ширину сетки поля(GridWidth),
        // Высоту сетки поля(GridHeight), Кол-во линий сетки поля(Rows) и Кол-во колонок сетки поля(Columns)
        public int Round
        {
            get
            {
                return _round;
            }
            set
            {
                _round = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Round)));
            }
        }
        public int TopScore
        {
            get
            {
                return _topScore;
            }
            set
            {
                _topScore = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TopScore)));
            }
        }
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Score)));
            }
        }
        public int GridWidth
        {
            get
            {
                return _gridWidth;
            }
            set
            {
                _gridWidth = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(GridWidth)));
            }
        }
        public int GridHeight
        {
            get
            {
                return _gridHeight;
            }
            set
            {
                _gridHeight = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(GridHeight)));
            }
        }
        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Rows)));
            }
        }
        public int Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Columns)));
            }
        }

        // EventHandler для поддержки и реализации ивентов
        public event PropertyChangedEventHandler PropertyChanged = (s, a) => { };
    }
}
