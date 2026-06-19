# Hangman Remake 🎮

## 📖 Overview
This repository contains a modern, interactive desktop version of the classic **Hangman** word-guessing game. Built with C# and **Windows Presentation Foundation (WPF)**, the application provides a graphical user interface and features multiple word categories, score tracking, and session management.

## 🧩 Architecture & Project Structure
The project is built using the **MVVM (Model-View-ViewModel)** architectural pattern to ensure clean, maintainable, and testable code by separating the user interface from the underlying game logic:

*   **Views (`MainWindow.xaml`, `GameWindow.xaml`)**: The XAML files handle the graphical interface, including the main menu and the actual game board.
*   **ViewModels (`MainWindowVM.cs`, `GameWindowVM.cs`)**: Act as the bridge between the UI and the data models. They handle commands (like button clicks for guessing letters) and update the UI via data binding.
*   **Models (`GameSession.cs`, `GameRecord.cs`, `LetterItem.cs`)**: Contain the core game logic, track the current session's state (lives remaining, guessed letters), and manage historical game records.
*   **Data Sources (`Cars.txt`, `Mountains.txt`)**: Text files utilized as local databases to load dynamically different categories of words for the player to guess.

## 🛠️ Technologies & Concepts
*   **Language:** C#
*   **Framework:** .NET / WPF (Windows Presentation Foundation)
*   **Design Pattern:** MVVM (Model-View-ViewModel)
*   **Concepts:** Data Binding, File I/O (reading words from text files), State Management.
