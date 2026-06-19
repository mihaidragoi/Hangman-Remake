using Hangman.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hangman.ViewModel
{
    class StatisticsVM
    {
        public ObservableCollection<GameRecord> AllMatches { get; set; }

        public StatisticsVM()
        {
            AllMatches = new ObservableCollection<GameRecord>();
            LoadData();
        }

        private void LoadData()
        {
            string filePath = "all_statistics.json";
            if (File.Exists(filePath))
            {

                    string json = File.ReadAllText(filePath);
                    var records = JsonSerializer.Deserialize<List<GameRecord>>(json);

                    if (records != null)
                        foreach (var record in records.OrderByDescending(r => r.Date))
                            AllMatches.Add(record);
            }
        }

        public void DeleteUserStatistics(string userName)
        {
            string filePath = "all_statistics.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var records = JsonSerializer.Deserialize<List<GameRecord>>(json);
                if (records != null)
                {
                    var updatedRecords = records.Where(r => r.PlayerName != userName).ToList();
                    File.WriteAllText(filePath, JsonSerializer.Serialize(updatedRecords));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
