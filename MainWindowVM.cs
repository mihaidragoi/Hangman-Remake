using Hangman.Model;
using Hangman.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Hangman.ViewModel
{
    class MainWindowVM : INotifyPropertyChanged
    {
        private ObservableCollection<User> users;

        public ObservableCollection<User> Users
        {
            get { return users; }
            set { users = value; }
        }

        private User? selectedUser;
        public User? SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                if (selectedUser != null)
                {
                    EnabledButtons = true;
                    currentAvatarIndex = allAvatars.IndexOf(selectedUser.ImagePath);
                    DisplayedAvatarPath = selectedUser.ImagePath;
                }
            }
        }

        public RelayCommand NewUserCommand { get; }
        public RelayCommand DeleteUserCommand { get; }
        public RelayCommand PlayCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool enabledButtons = false;
        public bool EnabledButtons
        {
            get { return enabledButtons; }
            set { enabledButtons = value; OnPropertyChanged(nameof(EnabledButtons)); }
        }
        public MainWindowVM()
        {
            users = new ObservableCollection<User>();
            NewUserCommand = new RelayCommand(o => NewUserImpl());
            DeleteUserCommand = new RelayCommand(o => DeleteUserImpl());
            PlayCommand = new RelayCommand(o => PlayImpl());
            CancelCommand = new RelayCommand(o => CancelImpl());
            ReloadFileUsers();

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars");
            if (Directory.Exists(path))
            {
                allAvatars = Directory.GetFiles(path, "*.png").ToList();
                if (allAvatars.Count > 0) DisplayedAvatarPath = allAvatars[0];
            }

            NextCommand = new RelayCommand(o => NextAvatarImpl(), o => allAvatars.Count > 0);
            PreviousCommand = new RelayCommand(o => PreviousAvatarImpl(), o => allAvatars.Count > 0);
        }
        private void NewUserImpl()
        {
            MessageBox.Show("You are creating a new user. Introduce the username, the avatar will the one displayed (selected before).", "New User", MessageBoxButton.OK, MessageBoxImage.Information);
            string userName = Microsoft.VisualBasic.Interaction.InputBox("Enter the name of the new user:", "New User", "User Name");

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("User name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            User newUser = new User(userName, DisplayedAvatarPath);
            Users.Add(newUser);
            SaveUsersToFile();
            EnabledButtons = true;

        }

        private void DeleteUserImpl()
        {
            if (SelectedUser == null) return;
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the user '{SelectedUser.Name}'?", "Delete User", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                string userNameToDelete = SelectedUser.Name;
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                string[] savedGames = Directory.GetFiles(directoryPath, $"{userNameToDelete}_*.json");
                foreach (string file in savedGames)
                    File.Delete(file);
                StatisticsVM statisticsVM = new StatisticsVM();
                statisticsVM.DeleteUserStatistics(userNameToDelete);
                Users.Remove(SelectedUser);
                SaveUsersToFile();
                SelectedUser = null;
                EnabledButtons = false;
                MessageBox.Show($"User '{userNameToDelete}' and all associated data have been deleted.", "User Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string selectedCategory = "All categories";
        public string SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));

                OnPropertyChanged(nameof(IsAllSelected));
                OnPropertyChanged(nameof(IsCarsSelected));
                OnPropertyChanged(nameof(IsMoviesSelected));
                OnPropertyChanged(nameof(IsStatesSelected));
                OnPropertyChanged(nameof(IsMountainsSelected));
                OnPropertyChanged(nameof(IsRiversSelected));

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
        private void PlayImpl()
        {
            if (SelectedUser == null) return;

            GameWindow gameWindow = new GameWindow();
            gameWindow.DataContext = new GameWindowVM(SelectedUser, SelectedCategory);

            gameWindow.Show();
            Application.Current.MainWindow.Hide();
        }

        private void CancelImpl()
        {
            Application.Current.Shutdown();
        }

        private string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
        private void ReloadFileUsers()
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
                return;
            }

            string jsonString = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(jsonString)) return;

            var deserializedUsers = JsonSerializer.Deserialize<ObservableCollection<User>>(jsonString);

            if (deserializedUsers != null)
            {
                Users.Clear();
                foreach (var user in deserializedUsers)
                {
                    Users.Add(user);
                }
            }
        }

        private void SaveUsersToFile()
        {
            string jsonString = JsonSerializer.Serialize(Users);
            File.WriteAllText(filePath, jsonString);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<string> allAvatars = new List<string>();
        private int currentAvatarIndex = 0;

        private string displayedAvatarPath;
        public string DisplayedAvatarPath
        {
            get { return displayedAvatarPath; }
            set
            {
                displayedAvatarPath = value;
                OnPropertyChanged(nameof(DisplayedAvatarPath));
            }
        }

        public RelayCommand NextCommand { get; }
        public RelayCommand PreviousCommand { get; }

        private void NextAvatarImpl()
        {
            if (allAvatars.Count == 0) return;
            currentAvatarIndex = (currentAvatarIndex + 1) % allAvatars.Count;
            DisplayedAvatarPath = allAvatars[currentAvatarIndex];
        }

        private void PreviousAvatarImpl()
        {
            if (allAvatars.Count == 0) return;
            currentAvatarIndex = (currentAvatarIndex - 1 + allAvatars.Count) % allAvatars.Count;
            DisplayedAvatarPath = allAvatars[currentAvatarIndex];
        }
    }
}