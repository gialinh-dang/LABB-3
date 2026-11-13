using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using LABB_3.ViewModel;

namespace LABB_3.Model;

public class Question : ViewModelBase
{
    private string _query = "";
    private int _correctIndex;

    public string Query
    {
        get => _query;
        set { _query = value; RaisePropertyChanged(); }
    }

    // Always 4 answers. CorrectIndex says which one is right (0-3)
    public List<string> Answers { get; set; } = new List<string> { "", "", "", "" };

    public int CorrectIndex
    {
        get => _correctIndex;
        set { _correctIndex = value; RaisePropertyChanged(); }
    }

    [JsonIgnore]
    public string CorrectAnswer => Answers[CorrectIndex];

    // A question can only be played when the text and all 4 answers are filled in
    [JsonIgnore]
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Query) &&
        Answers.Count == 4 &&
        Answers.All(answer => !string.IsNullOrWhiteSpace(answer));

    public Question() { }   // needed when JSON creates the object

    public Question(string query, string correctAnswer, string wrong1, string wrong2, string wrong3)
    {
        Query = query;
        Answers = new List<string> { correctAnswer, wrong1, wrong2, wrong3 };
        CorrectIndex = 0;
    }
}
