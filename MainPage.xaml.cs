using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Windows.Input;
using Microsoft.Maui.Storage;       // für Preferences
using MauiApp1.Views;               // ← hier den Namespace für AdminPage aufnehmen

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        // Die Basis-URL Deiner API-Routen
        private const string apiBase = "https://robbys-testumgebung.info/API.php?route=";

        // ICommand-Properties für XAML-Binds
        public ICommand OnLoginClickedCommand { get; }
        public ICommand OnRegisterTappedCommand { get; }

        public MainPage()
        {
            InitializeComponent();

            // Initialisiere die Commands
            OnLoginClickedCommand = new Command(async () => await OnLoginClicked());
            OnRegisterTappedCommand = new Command(async () => await OnRegisterTapped());

            // Setze das BindingContext, damit Commands & StatusLabel ansprechbar sind
            BindingContext = this;
        }

        /// <summary>
        /// Wird aufgerufen, wenn der "Anmelden"-Button gedrückt wird
        /// </summary>
        private async Task OnLoginClicked()
        {
            // Eingaben auslesen
            var username = UsernameEntry.Text?.Trim() ?? "";
            var password = PasswordEntry.Text ?? "";

            // Einfaches Validierungs-Check
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                StatusLabel.Text = "Bitte Benutzername und Passwort eingeben.";
                return;
            }

            try
            {
                var client = new HttpClient();

                // Login-Payload zusammenbauen
                var loginData = new { username = username, password = password };
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // POST-Aufruf an die Login-Route
                var response = await client.PostAsync(apiBase + "login", content);
                var body = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(body);

                // 1) Status prüfen
                if (response.IsSuccessStatusCode && data["status"]?.ToString() == "success")
                {
                    // 2) Token & Rolle aus der Rückmeldung auslesen
                    var token = data["token"]!.ToString();
                    // => Der API-Endpunkt muss inzwischen auch "role" zurückliefern (z.B. "ADMIN" oder "USER")
                    var role = data["role"]?.ToString() ?? "USER";

                    // 3) In Preferences speichern (damit andere Seiten darauf zugreifen können)
                    Preferences.Set("api_token", token);
                    Preferences.Set("api_role", role);

                    // 4) Erfolgsmeldung und Navigation je nach Rolle
                    if (role == "ADMIN")
                    {
                        await DisplayAlert("Erfolg", "Willkommen Admin!", "OK");
                        // Da wir MauiApp1.Views importiert haben, reicht hier new AdminPage():
                        await Navigation.PushAsync(new AdminPage());
                    }
                    else
                    {
                        await DisplayAlert("Erfolg", "Login erfolgreich – Willkommen!", "OK");
                        await Navigation.PushAsync(new UebersichtSeite());
                    }
                }
                else
                {
                    // Finden wir eine Fehlermeldung in "error" oder "message"
                    var msg = data["error"]?.ToString()
                              ?? data["message"]?.ToString()
                              ?? "Login fehlgeschlagen.";
                    StatusLabel.Text = "Login fehlgeschlagen: " + msg;
                }
            }
            catch (Exception ex)
            {
                // Netzwerk- oder Parsing-Fehler
                StatusLabel.Text = $"Fehler: {ex.Message}";
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn der "Hier registrieren"-Link angetippt wird.
        /// </summary>
        private async Task OnRegisterTapped()
        {
            // Navigiere zur RegisterPage
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
