// MauiApp1/ViewModels/MainPageViewModel.cs
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MauiApp1.ViewModels
{
    public class MainPageViewModel
    {
        // Hier definieren wir die beiden öffentlichen ICommand‐Properties:
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public MainPageViewModel()
        {
            // 1) LoginCommand: Was soll passieren, wenn man auf "Einloggen" klickt?
            LoginCommand = new Command(OnLogin);

            // 2) NavigateToRegisterCommand: Was soll passieren, wenn man "Hier registrieren" antippt?
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        }

        private async void OnLogin()
        {
            // TODO: Eure Logik, um den Login zu prüfen (z.B. API‐Aufruf, Validation, ...)
            // Beispiel:
            bool erfolg = await PrüfeLoginAsync();
            if (!erfolg)
            {
                // z.B. Benutzer informieren:
                await Application.Current.MainPage.DisplayAlert("Fehler", "Ungültige Zugangsdaten", "OK");
                return;
            }

            // Falls Login erfolgreich, z.B. Hauptseite öffnen:
            //await Shell.Current.GoToAsync("Hauptseite"); // je nach eurer Navigation
        }

        private async void OnNavigateToRegister()
        {
            // Beispiel: Mit Shell-Navigation zur Seite "RegisterPage" navigieren
            // Ihr müsst dafür in AppShell.xaml eine Route "RegisterPage" registriert haben.
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        private Task<bool> PrüfeLoginAsync()
        {
            // Platzhalter‐Implementation:
            return Task.FromResult(true);
        }
    }
}
