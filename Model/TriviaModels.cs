using System;
using System.Collections.Generic;
using System.Text;

namespace LABB_3.Model
{
    // These classes match the JSON from opentdb.com
    public class TriviaCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class TriviaCategoryResponse
    {
        public List<TriviaCategory> TriviaCategories { get; set; } = new List<TriviaCategory>();
    }

    public class TriviaQuestion
    {
        public string Question { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public List<string> IncorrectAnswers { get; set; } = new List<string>();
    }

    public class TriviaQuestionResponse
    {
        public int ResponseCode { get; set; }
        public List<TriviaQuestion> Results { get; set; } = new List<TriviaQuestion>();
    }

    public class TriviaApiException : Exception
    {
        public TriviaApiException(string message) : base(message) { }
    }
}
