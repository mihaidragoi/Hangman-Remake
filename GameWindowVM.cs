using Hangman.Model;
using Hangman.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hangman.ViewModel
{
    public class GameWindowVM : INotifyPropertyChanged
    {
        public ObservableCollection<char> WordDisplayList => currentGameSession.WordProgress;
        public ObservableCollection<string> ErrorSlots { get; set; }
        public string SelectedCategoryName => currentGameSession.Category;
        public ObservableCollection<LetterItem> Alphabet { get; set; }

        private void InitializeAlphabet()
        {
            Alphabet = new ObservableCollection<LetterItem>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Alphabet.Add(new LetterItem { Character = c });
            }
        }

        private GameSession currentGameSession;
        private User currentPlayer;

        public string PlayerName => currentGameSession.Username;
        public string? PlayerAvatar => currentPlayer?.ImagePath;
        public string CurrentLevelDisplay => $"Level: {currentGameSession.CurrentLevel}";

        private DispatcherTimer timer;
        private int timeLeft;

        public string timeToDisplay => $"Time left: {timeLeft}s";

        private void StartCountdown()
        {
            if (timer != null)
                timer.Stop();

            if(timeLeft <= 0)
                timeLeft = 30;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                timeLeft--;
                OnPropertyChanged(nameof(timeToDisplay));
                if (timeLeft <= 0)
                {
                    timer.Stop();
                    MessageBox.Show("Time's up!");
                    HandleLevelLoss();
                }
            };
            timer.Start();

        }

        public ICommand GuessLetterCommand { get; set; }

        public GameWindowVM(User selectedUser, string selectedCategory)
        {
            InitializeAlphabet();

            currentPlayer = selectedUser;
            currentGameSession = new GameSession
            {
                Username = selectedUser.Name,
                Category = selectedCategory,
                CurrentLevel = 1,
                TimeRemaining = 30,
                Mistakes = 0,
                WordProgress = new ObservableCollection<char>()
            };

            NewGameCommand = new RelayCommand(NewGameImpl);
            OpenGameCommand = new RelayCommand(OpenGameImpl);
            SaveGameCommand = new RelayCommand(SaveGameImpl);
            StatisticsCommand = new RelayCommand(StatisticsImpl);
            CancelCommand = new RelayCommand(CancelImpl);
            AboutCommand = new RelayCommand(AboutImpl);
            GuessLetterCommand = new RelayCommand(GuessLetterImpl);

            ErrorSlots = new ObservableCollection<string> { "", "", "", "", "", "" };
            SetupGame();
            StartCountdown();
            OnPropertyChanged(nameof(HangmanImage));
            OnPropertyChanged(nameof(CurrentLevelDisplay));
        }

        public string SelectedCategory
        {
            get => currentGameSession.Category;
            set
            {
                if (currentGameSession.Category == value) return;

                if (currentGameSession.CurrentLevel > 1)
                {
                    var result = MessageBox.Show("Changing the category will reset your current progress to Level 1. Do you want to continue?", "Change Category", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        OnPropertyChanged(nameof(SelectedCategory));
                        OnPropertyChanged(nameof(IsAllSelected));
                        OnPropertyChanged(nameof(IsCarsSelected));
                        OnPropertyChanged(nameof(IsMoviesSelected));
                        OnPropertyChanged(nameof(IsStatesSelected));
                        OnPropertyChanged(nameof(IsMountainsSelected));
                        OnPropertyChanged(nameof(IsRiversSelected));
                        return;
                    }
                }

                currentGameSession.Category = value;
                currentGameSession.CurrentLevel = 1; 

                OnPropertyChanged(nameof(SelectedCategory));
                OnPropertyChanged(nameof(CurrentLevelDisplay));
                OnPropertyChanged(nameof(IsAllSelected));
                OnPropertyChanged(nameof(IsCarsSelected));
                OnPropertyChanged(nameof(IsMoviesSelected));
                OnPropertyChanged(nameof(IsStatesSelected));
                OnPropertyChanged(nameof(IsMountainsSelected));
                OnPropertyChanged(nameof(IsRiversSelected));

                SetupGame();
            }
        }

        public bool IsAllSelected => SelectedCategory == "All categories";
        public bool IsCarsSelected => SelectedCategory == "Cars";
        public bool IsMoviesSelected => SelectedCategory == "Movies";
        public bool IsStatesSelected => SelectedCategory == "States";
        public bool IsMountainsSelected => SelectedCategory == "Mountains";
        public bool IsRiversSelected => SelectedCategory == "Rivers";

        public ICommand SelectCategoryCommand => new RelayCommand(param =>
        {
            if (param != null)
            {
                SelectedCategory = param.ToString();
            }
        });

        private void SetupGame()
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Words");
            List<string> words = new List<string>();

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Folderul 'Words' nu a fost găsit la: " + folderPath);
                return;
            }

            if (currentGameSession.Category == "All categories")
                foreach (var file in Directory.GetFiles(folderPath, "*.txt"))
                    words.AddRange(File.ReadAllLines(file));
            else
            {
                string filePath = Path.Combine(folderPath, $"{currentGameSession.Category}.txt");
                if (File.Exists(filePath))
                    words.AddRange(File.ReadAllLines(filePath));
            }

            if (words.Count > 0)
            {
                Random random = new Random();
                currentGameSession.SecretWord = words[random.Next(words.Count)].ToUpper();

                currentGameSession.WordProgress.Clear();
                foreach (var c in currentGameSession.SecretWord)
                {
                    currentGameSession.WordProgress.Add('_');
                }

                foreach (var letter in Alphabet) letter.IsAvailable = true;
            }
            else
                MessageBox.Show("Nu am găsit niciun cuvânt în fișierul corespunzător categoriei!");
            OnPropertyChanged(nameof(WordDisplayList));
        }

        public string HangmanImage
        {
            get
            {
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hangman");
                string imageName = $"hangman{currentGameSession.Mistakes}.png";
                return Path.Combine(basePath, imageName);
            }
        }

        private void GuessLetterImpl(object parameter)
        {
            if (parameter == null)
                return;

            char guessedChar;

            if (parameter is LetterItem letterItem)
                guessedChar = letterItem.Character;
            else
                if(parameter is string s)
                     guessedChar = s.ToUpper()[0];
            else
                return;

            var targetLetter = Alphabet.FirstOrDefault(l => l.Character == guessedChar);
            if (targetLetter == null || !targetLetter.IsAvailable)
                return;

            targetLetter.IsAvailable = false;
            isGameSaved = false;

            char c = targetLetter.Character;

            if (currentGameSession.SecretWord.Contains(c))
                {
                  for (int i = 0; i < currentGameSession.SecretWord.Length; i++)
                      if (currentGameSession.SecretWord[i] == c)
                          currentGameSession.WordProgress[i] = c;
                    if (!currentGameSession.WordProgress.Contains('_'))
                        HandleLevelWin();
                }

            else
                {
                  currentGameSession.Mistakes++;
                  ErrorSlots[currentGameSession.Mistakes - 1] = "X";
                  OnPropertyChanged(nameof(HangmanImage));
                  if (currentGameSession.Mistakes >= 6)
                      HandleLevelLoss();
                }
        }

        private void HandleLevelWin()
        {
            currentGameSession.CurrentLevel++;

            if (currentGameSession.CurrentLevel -1 == 3)
            {
                currentGameSession.CurrentLevel--;
                OnPropertyChanged(nameof(CurrentLevelDisplay));
                timer.Stop();
                timeLeft = 30;
                RecordMatch("Won");
                MessageBox.Show("Game won! You guessed three words in a row! Starting from level 1.");
                currentGameSession.CurrentLevel = 1;
                currentGameSession.Mistakes = 0;
                for (int i = 0; i < 6; i++) ErrorSlots[i] = "";

                OnPropertyChanged(nameof(CurrentLevelDisplay));
                OnPropertyChanged(nameof(timeToDisplay));
                OnPropertyChanged(nameof(HangmanImage));

                timer.Start();
                SetupGame();
                StartCountdown();

            }
            else
            {
                MessageBox.Show($"Level won! Get ready for the next word! Next level: {currentGameSession.CurrentLevel}");
                SetupGame();
                OnPropertyChanged(nameof(CurrentLevelDisplay));
            }
        }

        private void HandleLevelLoss()
        {
            RecordMatch("Lost");
            timer.Stop();
            MessageBox.Show("You lost! Starting from level 1!");
            currentGameSession.CurrentLevel = 1;
            currentGameSession.Mistakes = 0; 
            for (int i = 0; i < 6; i++) ErrorSlots[i] = ""; 
            SetupGame();
            StartCountdown();
            OnPropertyChanged(nameof(CurrentLevelDisplay));
            OnPropertyChanged(nameof(HangmanImage));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand NewGameCommand { get; set; }
        public ICommand OpenGameCommand { get; set; }
        public ICommand SaveGameCommand { get; set; }
        public ICommand StatisticsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AboutCommand { get; set; }

        private void NewGameImpl(object parameter)
        {
            if(isGameSaved == false)
            {
                var saveResult = MessageBox.Show("Do you want to save your current game before starting a new one?", "Save Game", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (saveResult == MessageBoxResult.Yes)
                    SaveGameImpl(null);
                else
                {
                    var result = MessageBox.Show("Are you sure you want to start a new game? Current progress will be lost.", "New Game", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        currentGameSession.CurrentLevel = 1;
                        currentGameSession.Mistakes = 0;
                        currentGameSession.Category = "All categories";
                        isGameSaved = false;
                        timeLeft = 30;
                        OnPropertyChanged(nameof(timeToDisplay));
                        SetupGame();
                        StartCountdown();
                        currentGameSession.CurrentLevel = 1;
                        currentGameSession.Mistakes = 0; 
                        for (int i = 0; i < 6; i++) ErrorSlots[i] = ""; 
                        OnPropertyChanged(nameof(CurrentLevelDisplay));
                    }
                }

            }
            else
               { 
                var result = MessageBox.Show("Are you sure you want to start a new game? This game is already saved.", "New Game", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    currentGameSession.CurrentLevel = 1;
                    currentGameSession.Mistakes = 0;
                    currentGameSession.Category = "All categories";
                    isGameSaved = false;
                    timeLeft = 30;
                    OnPropertyChanged(nameof(timeToDisplay));
                    SetupGame();
                    StartCountdown();
                    OnPropertyChanged(nameof(CurrentLevelDisplay));
                }
            }
        }

        private void OpenGameImpl(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                 string json = File.ReadAllText(openFileDialog.FileName);
                 GameSession? loadedSession = JsonSerializer.Deserialize<GameSession>(json);

                if (loadedSession != null)
                  {
                        if(loadedSession.Username != currentPlayer.Name)
                        {
                            MessageBox.Show("The loaded game belongs to a different player. Please select a compatible save file.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                    }
                        isGameSaved = true;

                        currentGameSession = loadedSession;
                        timeLeft = currentGameSession.TimeRemaining;

                        OnPropertyChanged(nameof(WordDisplayList));
                        OnPropertyChanged(nameof(PlayerName));
                        OnPropertyChanged(nameof(CurrentLevelDisplay));
                        OnPropertyChanged(nameof(SelectedCategory));
                        OnPropertyChanged(nameof(HangmanImage));

                        for (int i = 0; i < 6; i++)
                            ErrorSlots[i] = i < currentGameSession.Mistakes ? "X" : "";

                        foreach(var letter in Alphabet)
                            if(currentGameSession.WordProgress.Contains(letter.Character))
                                letter.IsAvailable = false;
                            else
                                letter.IsAvailable = true;


                    StartCountdown();
                        MessageBox.Show("Game Loaded Successfully!");
                }
             }
        }
        
        private bool isGameSaved = false;

        private void SaveGameImpl(object parameter)
        {
            currentGameSession.TimeRemaining = timeLeft;
            string fileName = $"{currentGameSession.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string json = JsonSerializer.Serialize(currentGameSession);
            File.WriteAllText(fileName, json);
            MessageBox.Show("Game saved successfully!");

            MessageBoxResult result = MessageBox.Show("Do you want to continue playing?", "Continue Playing", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                MessageBox.Show("Ok, the timer will stop and you can safely exit the game or load another one.", "Stop Playing", MessageBoxButton.OK, MessageBoxImage.Information);
                timer.Stop();
            }

            isGameSaved = true;

        }

        private void RecordMatch(string result)
        {
            string filePath = "all_statistics.json";
            List<GameRecord> history = new List<GameRecord>();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    history = JsonSerializer.Deserialize<List<GameRecord>>(json) ?? new List<GameRecord>();
                }

                history.Add(new GameRecord
                {
                    PlayerName = currentPlayer.Name,
                    Date = DateTime.Now,
                    Category = currentGameSession.Category,
                    Result = result,
                    LevelReached = currentGameSession.CurrentLevel
                });

                File.WriteAllText(filePath, JsonSerializer.Serialize(history));
        }
        private void StatisticsImpl(object parameter)
        {
            var statsWindow = new StatisticsWindow();
            statsWindow.ShowDialog();
        }

        private void CancelImpl(object parameter)
        {

            if(isGameSaved == false)
            {
                var saveResult = MessageBox.Show("Do you want to save your current game before exiting?", "Save Game", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (saveResult == MessageBoxResult.Yes)
                    SaveGameImpl(null);
                else
                {
                    var result = MessageBox.Show("Are you sure you want to exit the game? Current progress will be lost.", "Exit Game", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        Application.Current.MainWindow.Show();
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window is GameWindow)
                            {
                                window.Close();
                                break;
                            }
                        }
                    }
                    timer.Stop();
                }
            }

            else
             {                
            var result = MessageBox.Show("Are you sure you want to exit the game? Game is already saved.", "Exit Game", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.MainWindow.Show();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
                timer.Stop();
            }
        }

        private void AboutImpl(object parameter)
        {
            MessageBox.Show("Hangman Game \n Created by: Dragoi Mihai-Bogdan, Grupa 10LF242, Informatica", "About Hangman", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}