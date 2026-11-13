# Labb 3 – Math Quiz (Quiz Configurator)

A WPF app (.NET 10, MVVM) for creating question packs and playing quiz rounds.
Questions can also be imported from [Open Trivia Database](https://opentdb.com).

## How to use

| What | Menu | Shortcut | Button |
|---|---|---|---|
| New question pack | File → New Question Pack | Ctrl+N | **+ New Pack** |
| Import questions (API) | File → Import Questions | Ctrl+I | – |
| Add question | Edit → Add Question | Insert | **+** in the Questions list |
| Remove question | Edit → Remove Question | Delete | **Delete** |
| Pack options | Edit → Pack Options | Ctrl+O | Purple pen button |
| Play | View → Play | Ctrl+P | Green play button |
| Back to edit mode | View → Edit | Ctrl+E | Home button |

All menus can also be opened with Alt (for example Alt+E, O for Pack Options).

In play mode the questions and the answers come in a random order every time.
Each question has a countdown (set in Pack Options). After an answer, or when the time
runs out, the app shows if the answer was right and which answer is correct.
A pack can only be played when every question has a text and 4 answers.

## Project structure

```
Model/        Question, QuestionPack, Difficulty, Trivia API classes
ViewModel/    MainWindowViewModel, ConfigurationViewModel, PlayerViewModel, ImportViewModel
View/         MenuView, ConfigurationView, PlayerView (UserControls)
Dialogs/      PackOptionsDialog, CreateNewPackDialog, ImportQuestionDialog
Services/     JsonDataService, OpenTriviaService, DialogService
Command/      DelegateCommand
Converters/   IndexToBoolConverter (radio buttons <-> CorrectIndex)
```

## Technical solutions

- **MVVM:** all views use data binding and commands. `ViewModelBase` implements
  `INotifyPropertyChanged` and `DelegateCommand` implements `ICommand`. Code-behind only
  opens and closes windows.
- **DialogService:** ViewModels never create windows themselves. They ask `DialogService`
  to open a dialog, so the logic stays separate from the UI.
- **Saving (JSON):** all packs are saved asynchronously to
  `AppData\Local\Labb3MathQuiz\QuestionPacks.json`. The folder and file are created if they
  are missing, and a damaged file shows an error message instead of crashing the app.
  The app also saves when it is closed.
- **Open Trivia DB (VG):** categories and questions are loaded with `async`/`await`, so the
  UI never freezes. All API response codes (1–5) and other errors (no internet, timeout,
  unexpected data) are turned into messages the user can understand. HTML-encoded text from
  the API is decoded with `WebUtility.HtmlDecode`.

## A problem I solved

<!-- TODO: rewrite this part in your own words -->

**Problem:** After building the views, the project suddenly had 34 build errors, for example
`'UserControl1' already defines a member` and `InitializeComponent does not exist`.

**How I found the cause:** I read the errors one by one and compared the `x:Class` in each
XAML file with the namespace and class name in its code-behind. There were three causes:
1. My project is called `LABB 3`, so Visual Studio uses the namespace `LABB_3`, but part of
   my code used `Labb3`.
2. Several code-behind files still had the default class name `UserControl1`.
3. Even after fixing the code, old generated files in `obj` and unsaved editor tabs made the
   old errors come back.

**Solution:** I made every `x:Class` match its namespace and class name, closed the
solution without saving the old tabs, deleted `bin` and `obj`, and rebuilt. After that the
project built with 0 errors.
