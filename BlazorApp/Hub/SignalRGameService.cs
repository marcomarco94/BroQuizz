using System.ComponentModel;
using DataAccess;
using Microsoft.AspNetCore.SignalR;
using ModelsLibrary;

namespace BlazorApp.Hub;

public class SignalRGameService : IAsyncDisposable
{
    private readonly IDataService _dataService;
    private GameModel? _game;
    private readonly IHubContext<SignlalRHub> _hubContext;
    private List<PlayerModel?> _playersInQueue = new();
    private List<PlayerModel?> _playersUpdated = new();
    private Timer? _questionTimer;
    private List<QuestionModel?>? _quiz;
    private string? _roomId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SignalRGameService" /> class.
    /// </summary>
    /// <param name="dataService">The data service.</param>
    /// <param name="hubContext">The hub context.</param>
    public SignalRGameService(IDataService dataService, IHubContext<SignlalRHub> hubContext)
    {
        _dataService = dataService;
        _hubContext = hubContext;
    }

    public bool GameStarted => _game?.GameStarted ?? false;

    /// <summary>
    ///     Disposes the game service.
    /// </summary>
    public Action? OnDispose { get; set; }

    public ValueTask DisposeAsync()
    {
        if (_game != null) _game.PropertyChanged -= OnGamePropertyChanged;
        if (_questionTimer != null) _questionTimer.Dispose();
        OnDispose?.Invoke();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Starts the game with the given players and room ID.
    /// </summary>
    /// <param name="players">The players to start the game with.</param>
    /// <param name="roomId">The room ID.</param>
    public async Task StartGame(List<PlayerModel?> players, string? roomId)
    {
        _roomId = roomId;
        _playersUpdated = players;
        _quiz = await _dataService.GetQuizByTopicIdAsync(int.Parse(_roomId));
        if (!_quiz.Any()) return;

        _game = new GameModel(players);
        if (_game.Rounds > _quiz.Count) _game.Rounds = _quiz.Count;

        _game.Players = _playersUpdated;
        _game.RoundsLeft = _game.Rounds;
        _game.TimeLeftToAnswer = 10;
        _game.GameStarted = true;
        _game.PropertyChanged += OnGamePropertyChanged;
        await StartQuestionTimer();
    }

    private Task StartQuestionTimer()
    {
        _questionTimer = new Timer(NextQuestionByTime, null, 0, 1000);
        return Task.CompletedTask;
    }

    public async Task SendGameStarted(List<PlayerModel?> players)
    {
        if (GameStarted)
        {
            if (_game?.Players != null)
            {
                _playersInQueue = players.Except(_game.Players).ToList();
                _playersUpdated = players;
                var gameInQueue = new GameModel(_playersInQueue);
                gameInQueue.GameStarted = false;
                foreach (var player in _playersInQueue)
                    if (player?.ConnectionId != null)
                        await _hubContext.Clients.Client(player.ConnectionId).SendAsync("UpdateGame", gameInQueue);

                foreach (var player in _game.Players)
                    if (player?.ConnectionId != null)
                        await _hubContext.Clients.Client(player.ConnectionId).SendAsync("UpdateGame", _game);
            }
        }
    }

    private void NextQuestionByTime(object? state)
    {
        if (_game == null || (_game.RoundsLeft == 0 && _game.TimeLeftToAnswer <= -2))
        {
            Task.Run(async () => await PrePareNewGame());
        }
        else
        {
            if (_game.TimeLeftToAnswer == 0)
            {
                UpdateScore();
                _game.ShowCorrectAnswer = true;
            }
            else if (_game.TimeLeftToAnswer == -2)
            {
                _game.ShowCorrectAnswer = false;
                _game.TimeLeftToAnswer = _game.SecondsToAnswer;
            }

            if (_game.TimeLeftToAnswer == _game.SecondsToAnswer)
            {
                var index = _game.Rounds - _game.RoundsLeft;
                _game.ProgressCurrentQuestion = (int)((double)_game.TimeLeftToAnswer / _game.SecondsToAnswer * 100);
                _game.CurrentQuiz = _quiz?[index];
                _game.RoundsLeft--;
            }

            _game.TimeLeftToAnswer--;
            _game.ProgressCurrentQuestion = (int)((double)_game.TimeLeftToAnswer / _game.SecondsToAnswer * 100);
        }
    }

    private async Task PrePareNewGame()
    {
        if (_questionTimer != null)
            await _questionTimer.DisposeAsync();
        if (_game != null)
        {
            _game.ShowResult = true;
            await Task.Delay(10 * 1000);
            _game.ShowResult = false;
        }

        foreach (var player in _playersUpdated)
            if (player != null)
                player.Score = 0;
        await StartGame(_playersUpdated, _roomId);
    }


    /// <summary>
    ///     Ends the game.
    /// </summary>
    public async Task EndGame()
    {
        if (_game != null) _game.GameStarted = false;
        await DisposeAsync();
    }

    /// <summary>
    ///     Removes a player from the game.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    public void RemovePlayer(PlayerModel? player)
    {
        if (_game != null)
        {
            {
                var playerToRemove = _game.Players.FirstOrDefault(p => p?.ConnectionId == player?.ConnectionId);
                if (playerToRemove != null) _game.Players.Remove(playerToRemove);
            }

            var playerInQueue = _playersInQueue.FirstOrDefault(p => p?.ConnectionId == player?.ConnectionId);
            if (playerInQueue != null) _playersInQueue.Remove(playerInQueue);
            
            var playerUpdated = _playersUpdated.FirstOrDefault(p => p?.ConnectionId == player?.ConnectionId);
            if (playerUpdated != null) _playersUpdated.Remove(playerUpdated);
        }
    }

    /// <summary>
    ///     Updates the selected answer of a player.
    /// </summary>
    /// <param name="player">The player who selected the answer.</param>
    public void UpdateSelectedAnswer(PlayerModel? player)
    {
        if (_game is not null && _game.ShowCorrectAnswer == false)
                _game.Players.First(p => p?.ConnectionId == player?.ConnectionId)!.SelectedAnswer =
                    player?.SelectedAnswer;
    }

    private void ResetSelectedAnswer()
    {
        if (_game?.Players != null)
            foreach (var player in _game.Players)
                if (_game is not null && _game.ShowCorrectAnswer == false)
                    _game.Players.First(p => p.ConnectionId == player?.ConnectionId).SelectedAnswer = null;
    }

    private void UpdateScore()
    {
        if (_game?.Players != null)
            foreach (var player in _game.Players)
                if (player?.SelectedAnswer?.CorrectAnswer == true)
                {
                    player.Score++;
                    if (player.UserId != null && _game.CurrentQuiz?.Id != 0)
                        Task.Run(async () =>
                            await _dataService.UpdateUserDashboardAsync(player.UserId, _game.CurrentQuiz!.Id, true));
                }
                else
                {
                    if (player?.UserId != null && _game.CurrentQuiz?.Id != 0)
                        Task.Run(async () =>
                            await _dataService.UpdateUserDashboardAsync(player.UserId, _game.CurrentQuiz!.Id, false));
                }

        ResetSelectedAnswer();
    }

    private void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_game == null || _game.CurrentQuiz?.Answers == null) return;

        foreach (var player in _game.Players!)
                if (player?.ConnectionId != null)
                    _hubContext.Clients.Client(player.ConnectionId).SendAsync("UpdateGame", _game);
    }
}