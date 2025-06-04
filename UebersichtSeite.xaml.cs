using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;



namespace MauiApp1;

public partial class UebersichtSeite : ContentPage
{
    private HttpClient client = new();
    private string apiBase = "https://robbys-testumgebung.info/API.php";

    public UebersichtSeite()
    {
        InitializeComponent();
        LadeEinträge();
    }

    private async void LadeEinträge()
    {
        try
        {
            string token = Preferences.Get("api_token", "");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{apiBase}?route=/gesamt");
            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            if (data["data"] is JArray einträge)
            {
                var items = einträge.Select(d => $"#{d["id"]} – {d["inhalt_anfrage"]}: {d["beschreibung"]}").ToList();
                ErgebnisListe.ItemsSource = items;
            }
            else
            {
                ErgebnisListe.ItemsSource = new List<string> { "Keine Daten gefunden." };
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}
