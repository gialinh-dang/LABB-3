using LABB_3.Command;
using LABB_3.Model;
using LABB_3.Services;
using LABB_3.ViewModel;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;

namespace LABB_3.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _main;
        private readonly OpenTriviaService _triviaService = new OpenTriviaService();

        private TriviaCategory? _selectedCategory;
        private Difficulty _difficulty = Difficulty.Medium;
        private int _numberOfQuestions = 10;
        private bool _isBusy;
        private string _statusMessage = "";
        private bool _isError;

        public ObservableCollection<TriviaCategory> Categories { get; } = new ObservableCollection<TriviaCategory>();
        public Difficulty[] Difficulties { get; } = Enum.GetValues<Difficulty>();
        public string TargetPackName => _main.ActivePack?.Name ?? "";

        public TriviaCategory? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; RaisePropertyChanged(); }
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set { _difficulty = value; RaisePropertyChanged(); }
        }

        public int NumberOfQuestions
        {
            get => _numberOfQuestions;
            set { _numberOfQuestions = value; RaisePropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; RaisePropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; RaisePropertyChanged(); }
        }

        public bool IsError
        {
            get => _isError;
            set { _isError = value; RaisePropertyChanged(); }
        }

        public DelegateCommand ImportCommand { get; }

        public ImportViewModel(MainWindowViewModel main)
        {
            _main = main;
            ImportCommand = new DelegateCommand(async _ => await ImportAsync(),
                _ => !IsBusy && SelectedCategory != null && _main.ActivePack != null);
        }

        public async Task LoadCategoriesAsync()
        {
            IsBusy = true;
            ShowStatus("Loading categories...", false);

            try
            {
                var categories = await _triviaService.GetCategoriesAsync();
                foreach (var category in categories)
                    Categories.Add(category);

                SelectedCategory = Categories.FirstOrDefault();
                ShowStatus("", false);
            }
            catch (Exception ex)
            {
                ShowStatus(GetErrorMessage(ex), true);
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();   // update the Import button
            }
        }

        private async Task ImportAsync()
        {
            var pack = _main.ActivePack;
            if (pack == null || SelectedCategory == null) return;

            IsBusy = true;
            ShowStatus("Fetching questions...", false);

            try
            {
                var questions = await _triviaService.GetQuestionsAsync(SelectedCategory.Id, Difficulty, NumberOfQuestions);
                foreach (var question in questions)
                    pack.Questions.Add(question);

                await _main.SaveAsync();
                ShowStatus($"✓ {questions.Count} questions were added to \"{pack.Name}\".", false);
            }
            catch (Exception ex)
            {
                ShowStatus(GetErrorMessage(ex), true);
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsError = isError;
        }

        // Turn technical exceptions into messages a normal user understands
        private static string GetErrorMessage(Exception ex)
        {
            return ex switch
            {
                TriviaApiException => ex.Message,
                HttpRequestException => "Could not connect to Open Trivia DB. Check your internet connection.",
                TaskCanceledException => "The server took too long to answer. Please try again.",
                JsonException => "The server sent data in an unexpected format.",
                _ => "Something went wrong: " + ex.Message
            };
        }
    }
}
