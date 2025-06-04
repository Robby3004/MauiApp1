using Microsoft.Maui.Controls;   // (1)
using System.Net.Http;            // (2)
using System.Threading.Tasks;     // (3)
using MauiApp1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Windows.Input;


namespace MauiApp1.Views
{
    public partial class AdminPage : ContentPage
    {
        // Basis-URL deiner API (genau wie in Login/RegisterPage etc.)
        const string apiBase = "https://robbys-testumgebung.info/API.php?route=";

        // ObservableCollection, an die die ListView gebunden ist
        public ObservableCollection<User> PendingUsers { get; } = new();

        // Command, das ausgelöst wird, wenn der "Freigeben"-Button gedrückt wird
        public ICommand ApproveCommand { get; }

        public AdminPage()
        {
            InitializeComponent();

            // BindingContext der Seite auf sich selbst setzen
            BindingContext = this;

            // Command initialisieren (Lambda ruft die Methode ApproveAsync auf)
            ApproveCommand = new Command<int>(async userId => await ApproveAsync(userId));

            // ListView-ItemsSource auf die ObservableCollection setzen
            PendingList.ItemsSource = PendingUsers;

            // Pending-User-Liste beim Start laden
            _ = LoadPendingAsync();
        }

        /// <summary>
        /// Holt alle User, die noch nicht freigeschaltet (is_active=0) sind.
        /// </summary>
        async Task LoadPendingAsync()
        {
            try
            {
                var client = new HttpClient();

                // Token aus Preferences holen (vom Login gespeichert)
                var token = Preferences.Get("api_token", "");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                // API-Call: GET users_pending
                var response = await client.GetStringAsync(apiBase + "users_pending");
                var data = JObject.Parse(response);

                if (data["status"]?.ToString() == "success")
                {
                    // Liste vorher leeren
                    PendingUsers.Clear();

                    // Jedes JSON-Objekt in ein User-Objekt umwandeln und hinzufügen
                    foreach (var u in data["data"]!)
                    {
                        PendingUsers.Add(new User
                        {
                            id = (int)u["id"]!,
                            username = u["username"]!.ToString(),
                            email = u["email"]!.ToString()
                        });
                    }
                }
                else
                {
                    // Falls die API einen Fehler meldet
                    string msg = data["error"]?.ToString() ?? "Fehler beim Laden";
                    await DisplayAlert("Fehler", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Ruft die API auf, um einen User freizuschalten (user_activate).
        /// Nach erfolgreichem Freischalten wird der User aus der Liste entfernt.
        /// </summary>
        async Task ApproveAsync(int userId)
        {
            try
            {
                var client = new HttpClient();
                var token = Preferences.Get("api_token", "");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                // JSON-Payload erzeugen
                var payload = new { user_id = userId };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // API-Call: POST user_activate
                var resp = await client.PostAsync(apiBase + "user_activate", content);
                var body = await resp.Content.ReadAsStringAsync();
                var data = JObject.Parse(body);

                if (resp.IsSuccessStatusCode && data["status"]?.ToString() == "success")
                {
                    // Aus der Liste entfernen
                    var toRemove = PendingUsers.FirstOrDefault(u => u.id == userId);
                    if (toRemove != null)
                        PendingUsers.Remove(toRemove);
                }
                else
                {
                    string msg = data["error"]?.ToString() ?? data["message"]?.ToString() ?? "Fehler bei Freischaltung";
                    await DisplayAlert("Fehler", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }
}
