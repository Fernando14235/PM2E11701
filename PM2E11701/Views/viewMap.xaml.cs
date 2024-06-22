using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PM2E11701.Views;

public partial class viewMap : ContentPage
{
    public string photo;
    public viewMap(string txtLatitud, string txtLongitud, string foto)
	{
		InitializeComponent();
        photo = foto;
        if (double.TryParse(txtLatitud, out double latitud) && double.TryParse(txtLongitud, out double longitud))
        {
            var site = new Location(latitud, longitud);
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(site, Distance.FromMiles(0.5)));

            var pin = new Pin
            {
                Label = "Selected Location",
                Location = site,
                Type = PinType.Place
            };

            MyMap.Pins.Add(pin);
        }
        else
        {
            DisplayAlert("Error", "Coordenadas no Validas", "OK");
        }
    }

    private async Task SharePlaceAsync(string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir",
            File = new ShareFile(filePath)
        });
    }

    private async void btnShare_Clicked(object sender, EventArgs e)
    {
        byte[] imageBytes = Convert.FromBase64String(photo);
        string tempFilePath = Path.Combine(FileSystem.CacheDirectory, "tempImage.png");
        File.WriteAllBytes(tempFilePath, imageBytes);

        await SharePlaceAsync(tempFilePath);
    }


    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new placeList());
    }
}