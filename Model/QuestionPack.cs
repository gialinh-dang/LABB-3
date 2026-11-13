using LABB_3.Model;
using LABB_3.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace LABB_3.Model
{
    public class QuestionPack : ViewModelBase
    {
        private string _name = "";
        private Difficulty _difficulty = Difficulty.Medium;
        private int _timeLimitInSeconds = 20;

        public string Name
        {
            get => _name;
            set { _name = value; RaisePropertyChanged(); }
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set { _difficulty = value; RaisePropertyChanged(); }
        }

        public int TimeLimitInSeconds
        {
            get => _timeLimitInSeconds;
            set { _timeLimitInSeconds = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();

        public QuestionPack() { }

        public QuestionPack(string name, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 20)
        {
            Name = name;
            Difficulty = difficulty;
            TimeLimitInSeconds = timeLimitInSeconds;
        }
    }
}