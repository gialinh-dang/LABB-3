using LABB_3.Command;
using LABB_3.Model;
using LABB_3.Services;
using LABB_3.ViewModel;

namespace LABB_3.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private Question? _activeQuestion;

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

        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public ConfigurationViewModel(MainWindowViewModel main)
        {
            Main = main;

            AddQuestionCommand = new DelegateCommand(AddQuestion,
                _ => Main.IsConfigurationMode && Main.ActivePack != null);
            RemoveQuestionCommand = new DelegateCommand(RemoveQuestion,
                _ => Main.IsConfigurationMode && ActiveQuestion != null);
            SaveCommand = new DelegateCommand(async _ => await Main.SaveAsync());
        }

        private void AddQuestion(object? parameter)
        {
            var question = new Question { Query = "New question" };
            Main.ActivePack!.Questions.Add(question);
            ActiveQuestion = question;
        }

        private void RemoveQuestion(object? parameter)
        {
            var pack = Main.ActivePack;
            if (pack == null || ActiveQuestion == null) return;

            if (!DialogService.Confirm($"Delete the question\"{ActiveQuestion.Query}\"?")) return;

            pack.Questions.Remove(ActiveQuestion);
            ActiveQuestion = pack.Questions.FirstOrDefault();
        }
    }
}