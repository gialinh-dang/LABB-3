using LABB_3.Command;
using LABB_3.Model;
using LABB_3.Services;
using LABB_3.ViewModel;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;

namespace LABB_3.ViewModel
{

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly JsonDataService _dataService = new JsonDataService();

        private QuestionPack? _activePack;
        private QuestionPack? _newPack;
        private bool _isPlayMode;

        public ObservableCollection<QuestionPack> Packs { get; } = new ObservableCollection<QuestionPack>();

        public ConfigurationViewModel ConfigurationViewModel { get; }
        public PlayerViewModel PlayerViewModel { get; }

        // Lists used by the ComboBoxes in the dialogs
        public Difficulty[] Difficulties { get; } = Enum.GetValues<Difficulty>();
        public int[] TimeOptions { get; } = { 10, 15, 20, 30, 45, 60 };

        public QuestionPack? ActivePack
        {
            get => _activePack;
            set
            {
                _activePack = value;
                RaisePropertyChanged();
                ConfigurationViewModel.ActiveQuestion = value?.Questions.FirstOrDefault();
            }
        }

        // Temporary pack used by the "New Pack" dialog
        public QuestionPack? NewPack
        {
            get => _newPack;
            set { _newPack = value; RaisePropertyChanged(); }
        }

        public bool IsPlayMode
        {
            get => _isPlayMode;
            set
            {
                _isPlayMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsConfigurationMode));
            }
        }

        public bool IsConfigurationMode => !IsPlayMode;

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set { _isFullScreen = value; RaisePropertyChanged(); }
        }
        public DelegateCommand NewPackCommand { get; }
        public DelegateCommand DeletePackCommand { get; }
        public DelegateCommand PackOptionsCommand { get; }
        public DelegateCommand PlayCommand { get; }
        public DelegateCommand EditModeCommand { get; }
        public DelegateCommand ImportCommand { get; }
        public DelegateCommand ExitCommand { get; }
        public DelegateCommand ToggleFullScreenCommand { get; }
        public DelegateCommand ExitFullScreenCommand { get; }

        public MainWindowViewModel()
        {
            ConfigurationViewModel = new ConfigurationViewModel(this);
            PlayerViewModel = new PlayerViewModel(this);

            NewPackCommand = new DelegateCommand(CreateNewPack, _ => IsConfigurationMode);
            DeletePackCommand = new DelegateCommand(DeletePack, _ => IsConfigurationMode && Packs.Count > 1);
            PackOptionsCommand = new DelegateCommand(OpenPackOptions, _ => IsConfigurationMode && ActivePack != null);
            PlayCommand = new DelegateCommand(Play, CanPlay);
            EditModeCommand = new DelegateCommand(_ => ShowEditMode(), _ => IsPlayMode);
            ImportCommand = new DelegateCommand(
                _ => DialogService.ShowImportDialog(new ImportViewModel(this)),
                _ => IsConfigurationMode && ActivePack != null);
            ExitCommand = new DelegateCommand(_ => DialogService.CloseMainWindow());
            ToggleFullScreenCommand = new DelegateCommand(_ => IsFullScreen = !IsFullScreen);
            ExitFullScreenCommand = new DelegateCommand(_ => IsFullScreen = false, _ => IsFullScreen);
        }

        // ---------- Load / Save ----------

        public async Task LoadAsync()
        {
            try
            {
                var packs = await _dataService.LoadAsync();
                foreach (var pack in packs)
                    Packs.Add(pack);
            }
            catch (JsonException)
            {
                DialogService.ShowError("The save file is damaged and could not be read. Starting with a default pack.");
            }
            catch (IOException ex)
            {
                DialogService.ShowError($"Could not read the save file:\n{ex.Message}");
            }

            if (Packs.Count == 0)
            {
                Packs.Add(CreateDefaultPack());
                Packs.Add(CreateSubtractionPack());

            }
         
            ActivePack = Packs[0];
        }
        private static QuestionPack CreateSubtractionPack()
        {
            var pack = new QuestionPack("Subtraction (Easy)", Difficulty.Easy, 20);
            pack.Questions.Add(new Question("10 - 4 = ?", "6", "5", "7", "4"));
            pack.Questions.Add(new Question("15 - 8 = ?", "7", "6", "9", "8"));
            pack.Questions.Add(new Question("12 - 5 = ?", "6", "8", "7", "4"));
            return pack;
            
        }
        public async Task SaveAsync()
        {
            try
            {
                await _dataService.SaveAsync(Packs);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                DialogService.ShowError($"Could not save your question packs:\n{ex.Message}");
            }
        }

        // ---------- Packs ----------

        // Buttons in the pack list send their own pack as parameter.
        // Menu items and shortcuts send nothing, then we use the active pack.
        private QuestionPack? GetPack(object? parameter)
        {
            return parameter as QuestionPack ?? ActivePack;
        }

        private async void CreateNewPack(object? parameter)
        {
            var pack = new QuestionPack("New Pack");
            NewPack = pack;

            if (DialogService.ShowNewPackDialog(this))
            {
                Packs.Add(pack);
                ActivePack = pack;
                await SaveAsync();
            }
        }

        private async void DeletePack(object? parameter)
        {
            var pack = GetPack(parameter);
            if (pack == null) return;

            if (!DialogService.Confirm($"Do you really want to delete \"{pack.Name}\"?"))
                return;

            Packs.Remove(pack);

            // The ListBox sets ActivePack to null when the selected item is removed
            if (ActivePack == null || !Packs.Contains(ActivePack))
                ActivePack = Packs.FirstOrDefault();

            await SaveAsync();
        }

        private async void OpenPackOptions(object? parameter)
        {
            var pack = GetPack(parameter);
            if (pack == null) return;

            if (ActivePack != pack)
                ActivePack = pack;

            // Remember old values so "Cancel" can undo the changes
            string oldName = pack.Name;
            Difficulty oldDifficulty = pack.Difficulty;
            int oldTime = pack.TimeLimitInSeconds;

            if (DialogService.ShowPackOptionsDialog(this))
            {
                await SaveAsync();
            }
            else
            {
                pack.Name = oldName;
                pack.Difficulty = oldDifficulty;
                pack.TimeLimitInSeconds = oldTime;
            }
        }

        // ---------- Modes ----------

        private bool CanPlay(object? parameter)
        {
            var pack = GetPack(parameter);
            return IsConfigurationMode
                && pack != null
                && pack.Questions.Count > 0
                && pack.Questions.All(q => q.IsComplete);
        }

        private void Play(object? parameter)
        {
            var pack = GetPack(parameter);
            if (pack == null) return;

            if (ActivePack != pack)
                ActivePack = pack;

            IsPlayMode = true;
            PlayerViewModel.StartQuiz(pack);
        }

        public void ShowEditMode()
        {
            PlayerViewModel.StopQuiz();
            IsPlayMode = false;
        }

        private static QuestionPack CreateDefaultPack()
        {
            var pack = new QuestionPack("Addition (Easy)", Difficulty.Easy, 20);
            pack.Questions.Add(new Question("7 + 5 = ?", "12", "10", "13", "15"));
            pack.Questions.Add(new Question("3 + 4 = ?", "7", "6", "8", "9"));
            pack.Questions.Add(new Question("9 + 6 = ?", "15", "14", "16", "13"));
            return pack;
        }
    }
}
