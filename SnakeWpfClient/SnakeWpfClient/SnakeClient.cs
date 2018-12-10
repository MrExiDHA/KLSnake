using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace SnakeWpfClient
{
    public class SnakeClient
    {
        /*Top Secret super-puper private*/ private readonly string _token;
        private readonly RestClient _httpClient;

        public SnakeClient()
        {
            _token = "hqKteE%/(Y+Vj>eg2Wo[";
            _httpClient = new RestClient("http://safeboard.northeurope.cloudapp.azure.com");
        }

        // Отправление сообщения о директиве движения
        public async Task SendDirectionAsync(string dir)
        {
            var request = new RestRequest($"/api/Player/direction", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { direction = dir, token = _token });
            var responce = await _httpClient.ExecutePostTaskAsync(request);
        }

        // Получение карты
        public async Task<MapResponce> GetMapAsync()
        {
            var request = new RestRequest($"/api/Player/gameboard");
            var responce = await _httpClient.ExecuteGetTaskAsync<MapResponce>(request);
            return responce.Data;
        }

        // Получение имени
        public async Task<string> GetNameAsync()
        {
            var request = new RestRequest($"/api/Player/name");
            request.AddParameter("token", _token);
            var responce = await _httpClient.ExecuteGetTaskAsync<NameResponce>(request);
            return responce.Data.Name;
        }
    }

    // Группа классов и структур для выполнения контракта
    // и успешной десериализации полученного JSON-документа
    public sealed class NameResponce
    {
        public string Name { get; set; }
    }
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    public struct GameBoard
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public struct Wall
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public struct PlayerInfo
    {
        public string Name { get; set; }
        public bool IsSpawnProtected { get; set; }
        public IEnumerable<Point> Snake { get; set; }
    }
    public sealed class MapResponce
    {
        public bool IsStarted { get; set; }
        public bool IsPaused { get; set; }
        public int RoundNumber { get; set; }
        public int TurnNumber { get; set; }
        public int TurnTimeMilliseconds { get; set; }
        public int TimeUntilNextTurnMilliseconds { get; set; }
        public GameBoard GameBoardSize { get; set; }
        public int MaxFood { get; set; }
        public IEnumerable<PlayerInfo> Players { get; set; }
        public IEnumerable<Point> Food { get; set; }
        public IEnumerable<Wall> Walls { get; set; }
    }
}
