using System.ComponentModel;

namespace ModelsLibrary;

public class GameModel : INotifyPropertyChanged
{
    private QuestionModel? _currentQuiz;
    private bool _gameStarted;
    private List<PlayerModel>? _players;
    private int _progressCurrentQuestion;
    private int _rounds;
    private int _roundsLeft;
    private bool _showResult;

    public GameModel(List<PlayerModel?> players)
    {
        GameStarted = false;
        Players = new List<PlayerModel?>(players);
        CurrentQuiz = new QuestionModel();
        Rounds = 10;
        SecondsToAnswer = 10;
    }

    public int TimeLeftToAnswer { get; set; }
    public int SecondsToAnswer { get; set; }
    public bool ShowCorrectAnswer { get; set; }

    public int ProgressCurrentQuestion
    {
        get => _progressCurrentQuestion;
        set
        {
            _progressCurrentQuestion = value;
            OnPropertyChanged(nameof(ProgressCurrentQuestion));
        }
    }

    public bool ShowResult
    {
        get => _showResult;
        set
        {
            _showResult = value;
            OnPropertyChanged(nameof(ShowResult));
        }
    }

    public int Rounds
    {
        get => _rounds;
        set
        {
            _rounds = value;
            OnPropertyChanged(nameof(Rounds));
        }
    }

    public int RoundsLeft
    {
        get => _roundsLeft;
        set
        {
            _roundsLeft = value;
            OnPropertyChanged(nameof(RoundsLeft));
        }
    }

    public bool GameStarted
    {
        get => _gameStarted;
        set
        {
            _gameStarted = value;
            OnPropertyChanged(nameof(GameStarted));
        }
    }

    public List<PlayerModel?> Players
    {
        get => _players;
        set
        {
            _players = value;
            OnPropertyChanged(nameof(Players));
        }
    }

    public QuestionModel? CurrentQuiz
    {
        get => _currentQuiz;
        set
        {
            _currentQuiz = value;
            OnPropertyChanged(nameof(CurrentQuiz));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}