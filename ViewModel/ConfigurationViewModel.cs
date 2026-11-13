using LABB_3.Command;
using LABB_3.Model;
using LABB_3.Services;

namespace LABB_3.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private Question? _activeQuestion;
        private bool _isEditingQuestion;
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

        // true = show the "Edit Question" page, false = show the pack and question lists
        public bool IsEditingQuestion
        {
            get => _isEditingQuestion;
            set
            {
                _isEditingQuestion = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsShowingList));
            }
        }

        public bool IsShowingList => !IsEditingQuestion;

        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand EditQuestionCommand { get; }
        public DelegateCommand BackToListCommand { get; }

        public ConfigurationViewModel(MainWindowViewModel main)
        {
            Main = main;

            AddQuestionCommand = new DelegateCommand(AddQuestion,
                _ => Main.IsConfigurationMode && Main.ActivePack != null);
            RemoveQuestionCommand = new DelegateCommand(RemoveQuestion,
                _ => Main.IsConfigurationMode && ActiveQuestion != null);
            SaveCommand = new DelegateCommand(async _ => await SaveAndGoBack(),
                _ => ActiveQuestion != null && ActiveQuestion.IsComplete);
            EditQuestionCommand = new DelegateCommand(EditQuestion);
            BackToListCommand = new DelegateCommand(_ => BackToList());
        }

        private void EditQuestion(object? parameter)
        {
            // The pen button sends its own question as parameter
            if (parameter is Question question)
                ActiveQuestion = question;

            if (ActiveQuestion != null)
            {
                _isNewQuestion = false;
                IsEditingQuestion = true;
            }
        }

        private void AddQuestion(object? parameter)
        {
            var question = new Question { Query = "New question" };
            Main.ActivePack!.Questions.Add(question);
            ActiveQuestion = question;
            _isNewQuestion = true;
            IsEditingQuestion = true;   // open the new question right away
        }

        private void RemoveQuestion(object? parameter)
        {
            var pack = Main.ActivePack;
            if (pack == null || ActiveQuestion == null) return;

            if (!DialogService.Confirm($"Delete the question \"{ActiveQuestion.Query}\"?")) return;

            pack.Questions.Remove(ActiveQuestion);
            ActiveQuestion = pack.Questions.FirstOrDefault();
            IsEditingQuestion = false;  // the question is gone, go back to the list
        }

        private async Task SaveAndGoBack()
        {
            await Main.SaveAsync();
            _isNewQuestion = false;
            IsEditingQuestion = false;
        }

        private void BackToList()
        {
            // A new question that was never filled in should not stay in the pack
            if (_isNewQuestion && ActiveQuestion != null && !ActiveQuestion.IsComplete)
            {
                Main.ActivePack?.Questions.Remove(ActiveQuestion);
                ActiveQuestion = Main.ActivePack?.Questions.FirstOrDefault();
            }

            _isNewQuestion = false;
            IsEditingQuestion = false;
        }
    }
}
