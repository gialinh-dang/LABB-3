using LABB_3.Command;
using LABB_3.Model;
using LABB_3.Services;

namespace LABB_3.ViewModel
{
    // The three pages of the configuration mode
    public enum ConfigurationPage
    {
        Home,           // page 0: start page with Play / Question Packs / Settings
        Packs,          // page 1: list of question packs
        Questions,      // page 2: questions in the active pack
        EditQuestion    // page 3: edit one question
    }

    public class ConfigurationViewModel : ViewModelBase
    {
        private Question? _activeQuestion;
        private ConfigurationPage _currentPage = ConfigurationPage.Home;
        private bool _isSettingsMode;  // false = packs page from Play, true = packs page from Settings
        private bool _isNewQuestion;   // true while the question from the + button is not saved yet

        public MainWindowViewModel Main { get; }

        public Question? ActiveQuestion
        {
            get => _activeQuestion;
            set
            {
                _activeQuestion = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasActiveQuestion));
            }
        }

        public bool HasActiveQuestion => ActiveQuestion != null;

        // Which page is shown right now. The View shows/hides the pages with these bools.
        public ConfigurationPage CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsHomePage));
                RaisePropertyChanged(nameof(IsPacksPage));
                RaisePropertyChanged(nameof(IsQuestionsPage));
                RaisePropertyChanged(nameof(IsEditQuestionPage));
            }
        }

        public bool IsHomePage => CurrentPage == ConfigurationPage.Home;

        // The packs page looks different when it is opened from Play or from Settings
        public bool IsSettingsMode
        {
            get => _isSettingsMode;
            set
            {
                _isSettingsMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsChoosingPackToPlay));
                RaisePropertyChanged(nameof(PacksPageTitle));
                RaisePropertyChanged(nameof(PacksPageTip));
            }
        }

        public bool IsChoosingPackToPlay => !IsSettingsMode;

        public string PacksPageTitle => IsSettingsMode ? "Question Packs" : "Choose a Pack";

        public string PacksPageTip => IsSettingsMode
            ? "Tip: click a pack to see and edit its questions · Ctrl+N = new pack"
            : "Tip: press the green button to play a pack";
        public bool IsPacksPage => CurrentPage == ConfigurationPage.Packs;
        public bool IsQuestionsPage => CurrentPage == ConfigurationPage.Questions;
        public bool IsEditQuestionPage => CurrentPage == ConfigurationPage.EditQuestion;

        public DelegateCommand ShowPacksCommand { get; }
        public DelegateCommand GoHomeCommand { get; }
        public DelegateCommand OpenSettingsCommand { get; }
        public DelegateCommand OpenPackCommand { get; }
        public DelegateCommand BackToPacksCommand { get; }
        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand EditQuestionCommand { get; }
        public DelegateCommand BackToListCommand { get; }

        public ConfigurationViewModel(MainWindowViewModel main)
        {
            Main = main;

            ShowPacksCommand = new DelegateCommand(_ => ShowPacksToPlay());
            GoHomeCommand = new DelegateCommand(_ => CurrentPage = ConfigurationPage.Home);
            OpenSettingsCommand = new DelegateCommand(OpenSettings);
            OpenPackCommand = new DelegateCommand(OpenPack);
            BackToPacksCommand = new DelegateCommand(_ => CurrentPage = ConfigurationPage.Packs);
            AddQuestionCommand = new DelegateCommand(AddQuestion,
                _ => Main.IsConfigurationMode && Main.ActivePack != null);
            RemoveQuestionCommand = new DelegateCommand(RemoveQuestion,
                _ => Main.IsConfigurationMode && ActiveQuestion != null);
            SaveCommand = new DelegateCommand(async _ => await SaveAndGoBack(),
                _ => ActiveQuestion != null && ActiveQuestion.IsComplete);
            EditQuestionCommand = new DelegateCommand(EditQuestion);
            BackToListCommand = new DelegateCommand(_ => BackToList());
        }

        // Home -> Play: show the packs with only the green play button
        public void ShowPacksToPlay()
        {
            IsSettingsMode = false;
            CurrentPage = ConfigurationPage.Packs;
        }

        // Home -> Settings: show the packs with the pen and trash buttons
        private void OpenSettings(object? parameter)
        {
            IsSettingsMode = true;
            CurrentPage = ConfigurationPage.Packs;
        }

        // Page 1 -> page 2: the pen button on a pack sends its own pack as parameter
        private void OpenPack(object? parameter)
        {
            // Only the Settings page opens the questions, the Play page only plays
            if (!IsSettingsMode) return;

            if (parameter is QuestionPack pack)
                Main.ActivePack = pack;

            if (Main.ActivePack != null)
                CurrentPage = ConfigurationPage.Questions;
        }

        // Page 2 -> page 3: the pen button on a question sends its own question as parameter
        private void EditQuestion(object? parameter)
        {
            if (parameter is Question question)
                ActiveQuestion = question;

            if (ActiveQuestion != null)
            {
                _isNewQuestion = false;
                CurrentPage = ConfigurationPage.EditQuestion;
            }
        }

        private void AddQuestion(object? parameter)
        {
            var question = new Question { Query = "New question" };
            Main.ActivePack!.Questions.Add(question);
            ActiveQuestion = question;
            _isNewQuestion = true;
            CurrentPage = ConfigurationPage.EditQuestion;   // open the new question right away
        }

        private void RemoveQuestion(object? parameter)
        {
            var pack = Main.ActivePack;
            if (pack == null || ActiveQuestion == null) return;

            if (!DialogService.Confirm($"Delete the question \"{ActiveQuestion.Query}\"?")) return;

            pack.Questions.Remove(ActiveQuestion);
            ActiveQuestion = pack.Questions.FirstOrDefault();
            _isNewQuestion = false;

            // The question is gone, so the edit page has nothing to show any more
            if (CurrentPage == ConfigurationPage.EditQuestion)
                CurrentPage = ConfigurationPage.Questions;
        }

        private async Task SaveAndGoBack()
        {
            await Main.SaveAsync();
            _isNewQuestion = false;
            CurrentPage = ConfigurationPage.Questions;
        }

        // Page 3 -> page 2
        private void BackToList()
        {
            // A new question that was never filled in should not stay in the pack
            if (_isNewQuestion && ActiveQuestion != null && !ActiveQuestion.IsComplete)
            {
                Main.ActivePack?.Questions.Remove(ActiveQuestion);
                ActiveQuestion = Main.ActivePack?.Questions.FirstOrDefault();
            }

            _isNewQuestion = false;
            CurrentPage = ConfigurationPage.Questions;
        }
    }
}
