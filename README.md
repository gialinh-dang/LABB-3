# Labb 3 – Math Adventure

This is my Labb 3 in the C# and .NET course. It's a quiz app made in WPF with MVVM.
You can make your own question packs, play them, and also import questions from
Open Trivia Database.

I wanted it to look like a quiz game for kids, so I went with light colors, rounded
buttons and a sky with green hills in the background.

## How the app works

When you start the app you come to the start page. There are two buttons:

- **Play** – shows the list of packs. Every pack has a green play button and that's it.
- **Settings** – shows the same packs, but here you can edit them. Click on a pack to
  see its questions, and click the pen next to a question to edit it.

Every page has a back arrow so you can go back one step. The quiz itself doesn't have
one, the idea is that you finish the round first. When the round is done you can play
again or press Menu to get back to the list of packs.

The app starts with four packs: addition, subtraction, multiplication and division.
These are only created the first time (when there is no save file yet).

### Playing

- The questions come in a random order, and so do the four answers.
- Every question has a timer. You set the time per question in Pack Options.
- When you answer, or when the time runs out, the right answer gets a check mark and
  a wrong answer gets an X. After two seconds the next question shows up.
- At the end you see how many you got right and the percentage.

### Editing

- In the edit page you write the question and four answers, and pick the right answer
  with the round button.
- You can't save a question until all four answers are filled in.
- If you press + and then go back without writing anything, the empty question is
  removed again.
- A pack can't be played if one of its questions is missing an answer.
- Deleting a question asks you first.

### Keyboard shortcuts

All menus also work with Alt, for example Alt+E, O for Pack Options.

| What | Shortcut |
|---|---|
| New question pack | Ctrl+N |
| Import questions | Ctrl+I |
| Pack options | Ctrl+O |
| Add question | Insert |
| Remove question | Delete |
| Play | Ctrl+P |
| Back to edit mode | Ctrl+E |
| Full screen on/off | F11 (Esc to leave) |

Deleting a whole pack is done from File → Delete Question Pack.

### Importing questions (VG part)

File → Import Questions (Ctrl+I) opens a window where you pick a category, difficulty
and how many questions you want (1–50). The questions are added to the pack you have
selected.

The app tells you if something goes wrong, for example if you have no internet, if the
server is too slow, or if there aren't enough questions in that category. It also
handles the response codes from the API, like code 5 when you send requests too fast.

## How the code is built

```
Model/        Question, QuestionPack, Difficulty and the classes for the API data
ViewModel/    MainWindowViewModel, ConfigurationViewModel, PlayerViewModel, ImportViewModel
View/         HomeView, MenuView, ConfigurationView, PlayerView
Dialogs/      PackOptionsDialog, CreateNewPackDialog, ImportQuestionDialog
Services/     JsonDataService, OpenTriviaService, DialogService
Command/      DelegateCommand
Converters/   IndexToBoolConverter
```

Some things I want to explain:

- **MVVM.** The views only use bindings and commands. `ViewModelBase` has
  `INotifyPropertyChanged` and `DelegateCommand` is my `ICommand`. There is almost no
  code in the code-behind files, only things like closing a dialog or dragging it.
- **DialogService.** I didn't want the ViewModels to create windows, so they call
  `DialogService` instead, and that class is the only one that opens dialogs.
- **Pages.** The configuration part has four pages (start, packs, questions, edit).
  `ConfigurationViewModel` has a `CurrentPage` enum and the view shows the right page
  with `Visibility` bindings.
- **Saving.** All packs are saved as JSON in
  `AppData\Local\Labb3MathQuiz\QuestionPacks.json`. Reading and writing is async. If
  the folder or file doesn't exist it gets created. If the file can't be read the app
  shows a message and starts with the default packs instead of crashing. It saves when
  you press Save, when you change packs, and when you close the app.
- **Radio buttons.** The right answer is stored as an index (0–3). I made
  `IndexToBoolConverter` so each radio button can be bound to that same number.
- **API.** `OpenTriviaService` uses one `HttpClient` for the whole app. The text from the
  API has HTML codes like `&quot;` so I run it through `WebUtility.HtmlDecode`.

## A problem I had

When I started putting the code together I suddenly got 34 errors, things like
`'UserControl1' already defines a member` and `InitializeComponent does not exist`.

I went through the errors one by one and compared the `x:Class` in every XAML file with
the namespace and class name in its `.xaml.cs` file. It turned out there were a few
things wrong at the same time:

1. My project is called "LABB 3" with a space, so Visual Studio made the namespace
   `LABB_3`. Some of my files used `Labb3`, so they didn't match.
2. Some code-behind files still had the default class name `UserControl1`.
3. Some XAML files had a `Window` tag where it should be a `UserControl`.

I fixed the namespaces with Find and Replace and gave every code-behind the right class
name. But the errors still came back. The reason was that Visual Studio still had old
unsaved versions of some files open, and the `obj` folder had old generated files in it.
So I closed the solution without saving, deleted `bin` and `obj`, and rebuilt. After
that it built with 0 errors.

What I learned: `x:Class`, the namespace and the class name always have to match. And
if errors keep coming back even when the code looks right, try a clean rebuild before
changing more code.
