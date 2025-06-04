using System.Text;                            // für Encoding.UTF8
using System.Net.Http.Headers;                // für MediaTypeHeaderValue
using Newtonsoft.Json;                        // falls noch nicht drin
using Newtonsoft.Json.Linq;


namespace MauiApp1;

public partial class RegisterPage : ContentPage
{
    private const string ApiBaseUrl = "https://robbys-testumgebung.info/API.php";

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = string.Empty;

        var regData = new
        {
            username = NameEntry.Text?.Trim(),
            email = EmailEntry.Text?.Trim(),
            password = PasswordEntry.Text
        };

        // Einfache Validierung
        if (string.IsNullOrWhiteSpace(regData.username) ||
            string.IsNullOrWhiteSpace(regData.email) ||
            string.IsNullOrWhiteSpace(regData.password))
        {
            StatusLabel.Text = "Alle Felder ausfüllen.";
            return;
        }

        var client = new HttpClient();
        var json = JsonConvert.SerializeObject(regData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{ApiBaseUrl}?route=register", content);
            var body = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(body);

            if (response.IsSuccessStatusCode && data["status"]?.ToString() == "success")
            {
                await DisplayAlert("Erfolg", data["message"]!.ToString(), "OK");
                await Navigation.PopAsync(); // zurück zum Login
            }
            else
            {
                StatusLabel.Text = data["message"]!.ToString();
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Fehler: {ex.Message}";
        }
    }

    private async void OnGoToLogin(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
