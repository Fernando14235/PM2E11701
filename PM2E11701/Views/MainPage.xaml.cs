using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Net.Http;

//using Xamarin.Forms;
namespace PM2E11701.Views;


public partial class MainPage : ContentPage
{

    private const string GoogleMapsApiKey = "AIzaSyBn2tnAFfuslQLUfqsqKmvQsZVFbv_CutU";

    Controllers.PlaceController controller;
    FileResult photo; //Para tomar foto


    public MainPage()
	{
		InitializeComponent();

        controller = new Controllers.PlaceController();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location != null)
            {
                txtLatitud.Text = location.Latitude.ToString();
                txtLongitud.Text = location.Longitude.ToString();

                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    txtDescrip.Text = placemark.Thoroughfare + ", " + placemark.Locality; // Ejemplo de construcción de la descripción del lugar
                }
                else
                {
                   txtDescrip.Text = "No location description available";
                }
            }
            else
            {
                await DisplayAlert("Error", "Unable to get location", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
        await CheckGpsStatusAsync();
    }

    private async System.Threading.Tasks.Task CheckGpsStatusAsync()
    {
        var locationStatus = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

        if (locationStatus == null)
        {
            await DisplayAlert("GPS Not Enabled", "Please enable GPS to use this app.", "OK");
        }
    }



    public string? GetImg64()
    {
        if (photo != null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream stream = photo.OpenReadAsync().Result;
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();

                String Base64 = Convert.ToBase64String(data);

                return Base64;
            }
        }
        return null;
    }

    public byte[]? GetImageArray()
    {
        if (photo != null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream stream = photo.OpenReadAsync().Result;
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();

                return data;
            }
        }
        return null;
    }


    private void btnSitios_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new placeList());

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourcephoto = await photo.OpenReadAsync();
            using FileStream streamlocal = File.OpenWrite(photoPath);

            imgFoto.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result); //Para verla dentro de archivo

            await sourcephoto.CopyToAsync(streamlocal); //Para Guardarla local
        }
    }

    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string Descripcion = txtDescrip.Text;

        if (string.IsNullOrEmpty(Descripcion))
        {
            await DisplayAlert("Error", "Porfavor ingrese la ciudad", "OK");
            return;
        }
        
        var autor = new Models.Place
        {
            Longitud = txtLongitud.Text,
            Latitud = txtLatitud.Text,
            Descripcion = txtDescrip.Text,
            Foto = GetImg64()
        };

        try
        {
            if (controller != null)
            {
                if (await controller.storePlace(autor) > 0)
                {
                    await DisplayAlert("Aviso", "Registro Ingresado con Exito!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrio un Error", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrio un Error: {ex.Message}", "OK");
        }
    }

    private void OnLinkTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers[0] is TapGestureRecognizer tapGestureRecognizer)
        {
            string url = tapGestureRecognizer.CommandParameter as string;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Uri uri = new Uri(url);
                    Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)

                {

                }
            }
        }
    }
    
    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        // Termina la aplicación
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        // También puedes usar Environment.Exit(0) en lugar de Kill()
        // Environment.Exit(0);
    }

    private async void btnLista_Clicked(object sender, EventArgs e)
    {
        // Lógica para manejar el evento del botón Lista
        await Navigation.PushAsync(new placeList());
    }

}