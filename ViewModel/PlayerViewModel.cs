using LABB_3.Command;
using LABB_3.Model;
using LABB_3.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace LABB_3.ViewModel;

public class PlayerViewModel : ViewModelBase
{
    private readonly DispatcherTimer _timer = new DispatcherTimer();
    private readonly string[] _buttonColors = { "#FFB13D", "#4CC26A", "#A77BEA", "#FF7FA3" };

    private QuestionPack? _pack;
    private List<Question> _questions = new List<Question>();
    private int _round;   // changes every time a quiz starts or stops

    private Question? _currentQuestion;
    private int _questionNumber;
    private int _totalQuestions;
    private int _timeLeft;
    private int _correctCount;
    private bool _isAnswered;
    private bool _isFinished;
    private string _feedbackText = "";
    private string _resultMessage = "";

    public MainWindowViewModel Main { get; }
    public ObservableCollection<AnswerOption> Answers { get; } = new ObservableCollection<AnswerOption>();

    public Question? CurrentQuestion
    {
        get => _currentQuestion;
        set { _currentQuestion = value; RaisePropertyChanged(); }
    }

    public int QuestionNumber
    {
        get => _questionNumber;
        set { _questionNumber = value; RaisePropertyChanged(); }
    }

    public int TotalQuestions
    {
        get => _totalQuestions;
        set { _totalQuestions = value; RaisePropertyChanged(); }
    }

    public int TimeLeft
    {
        get => _timeLeft;
        set
        {
            _timeLeft = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(TimeText));
            RaisePropertyChanged(nameof(IsTimeLow));
        }
    }

    public string TimeText => TimeSpan.FromSeconds(TimeLeft).ToString(@"mm\:ss");
    public bool IsTimeLow => TimeLeft <= 5;

    public int CorrectCount
    {
        get => _correctCount;
        set { _correctCount = value; RaisePropertyChanged(); }
    }

    public bool IsAnswered
    {
        get => _isAnswered;
        set { _isAnswered = value; RaisePropertyChanged(); }
    }

    public bool IsFinished
    {
        get => _isFinished;
        set
        {
            _isFinished = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsPlaying));
        }
    }

    public bool IsPlaying => !IsFinished;

    public string FeedbackText
    {
        get => _feedbackText;
        set { _feedbackText = value; RaisePropertyChanged(); }
    }

    public string ResultMessage
    {
        get => _resultMessage;
        set { _resultMessage = value; RaisePropertyChanged(); }
    }

    public string ScoreText => $"{CorrectCount} / {TotalQuestions}";

    public DelegateCommand AnswerCommand { get; }
    public DelegateCommand PlayAgainCommand { get; }
    public DelegateCommand BackToMenuCommand { get; }

    public PlayerViewModel(MainWindowViewModel main)
    {
        Main = main;

        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;   // subscribe only ONCE

        AnswerCommand = new DelegateCommand(p => CheckAnswer(p as AnswerOption));
        PlayAgainCommand = new DelegateCommand(_ => { if (_pack != null) StartQuiz(_pack); });
        BackToMenuCommand = new DelegateCommand(_ => Main.ShowEditMode());
    }

    public void StartQuiz(QuestionPack pack)
    {
        _round++;
        _pack = pack;

        // Random order of the questions every time
        _questions = pack.Questions.OrderBy(q => Random.Shared.Next()).ToList();

        TotalQuestions = _questions.Count;
        QuestionNumber = 0;
        CorrectCount = 0;
        IsFinished = false;

        ShowNextQuestion();
    }

    public void StopQuiz()
    {
        _round++;
        _timer.Stop();
    }

    private void ShowNextQuestion()
    {
        if (QuestionNumber >= TotalQuestions)
        {
            FinishQuiz();
            return;
        }

        var question = _questions[QuestionNumber];
        CurrentQuestion = question;
        QuestionNumber++;
        FeedbackText = "";
        IsAnswered = false;

        // Random order of the answers: shuffle the positions 0,1,2,3
        var order = new List<int> { 0, 1, 2, 3 }.OrderBy(i => Random.Shared.Next()).ToList();

        Answers.Clear();
        for (int i = 0; i < 4; i++)
        {
            int index = order[i];
            bool isCorrect = index == question.CorrectIndex;
            Answers.Add(new AnswerOption(question.Answers[index], isCorrect, _buttonColors[i]));
        }

        TimeLeft = _pack!.TimeLimitInSeconds;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        TimeLeft--;
        if (TimeLeft <= 0)
            CheckAnswer(null);   // null = no answer, time is up
    }

    private async void CheckAnswer(AnswerOption? picked)
    {
        if (IsAnswered) return;   // ignore extra clicks

        IsAnswered = true;
        _timer.Stop();

        string correctAnswer = CurrentQuestion?.CorrectAnswer ?? "";

        if (picked == null)
        {
            FeedbackText = $"Time's up! The right answer is {correctAnswer}.";
        }
        else if (picked.IsCorrect)
        {
            CorrectCount++;
            FeedbackText = "Correct, well done!";
        }
        else
        {
            picked.IsPicked = true;
            FeedbackText = $"Oops! The right answer is {correctAnswer}.";
        }

        foreach (var answer in Answers)
            answer.ShowResult = true;

        // Wait 2 seconds so the player can see the feedback
        int round = _round;
        await Task.Delay(2000);

        // If the player left or restarted during the wait, do nothing
        if (round == _round)
            ShowNextQuestion();
    }

    private void FinishQuiz()
    {
        _timer.Stop();

        double percent = (double)CorrectCount / TotalQuestions;
        if (percent >= 0.8)
            ResultMessage = "Great job!";
        else if (percent >= 0.5)
            ResultMessage = "Good try!";
        else
            ResultMessage = "Keep practicing!";

        RaisePropertyChanged(nameof(ScoreText));
        IsFinished = true;
    }
}