using System;
using System.Collections.Generic;
using System.Text;

namespace LABB_3.ViewModel;

public class AnswerOption : ViewModelBase
{
    private bool _showResult;

    public string Text { get; }
    public bool IsCorrect { get; }
    public string Color { get; }
    public bool IsPicked { get; set; }

    public AnswerOption(string text, bool isCorrect, string color)
    {
        Text = text;
        IsCorrect = isCorrect;
        Color = color;
    }

    // Becomes true after the player answered (or time ran out)
    public bool ShowResult
    {
        get => _showResult;
        set
        {
            _showResult = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(ShowCheck));
            RaisePropertyChanged(nameof(ShowCross));
            RaisePropertyChanged(nameof(Opacity));
        }
    }

    public bool ShowCheck => ShowResult && IsCorrect;              // green check on the right answer
    public bool ShowCross => ShowResult && IsPicked && !IsCorrect; // X on the wrong answer you picked
    public double Opacity => ShowResult && !IsCorrect && !IsPicked ? 0.45 : 1.0;
}