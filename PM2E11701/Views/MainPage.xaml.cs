using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using PM2E11701.Views;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Controls.PlatformConfiguration;
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
        UpdateLocationState();

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

                var placements = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placement = placements?.FirstOrDefault();

                if (placement != null)
                {
                    txtDescrip.Text = placement.Locality; 
                }
                else
                {
                   txtDescrip.Text = "No location description available";
                }
            }
            else
            {
                await DisplayAlert("Error", "No se puede obtener la ubicacion", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
        await CheckGpsStatusAsync();
        UpdateLocationState();
    }

    private async System.Threading.Tasks.Task CheckGpsStatusAsync()
    {
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

        if (location == null)
        {
            await DisplayAlert("GPS No Activado", "Por favor active el GPS para usarlo en esta app.", "OK");
        }
    }

    private bool AreAllFieldsValid()
    {
        if (string.IsNullOrEmpty(txtLatitud.Text))
        {
            return false;
        }

        if (string.IsNullOrEmpty(txtLongitud.Text))
        {
            return false;
        }

        if (string.IsNullOrEmpty(txtDescrip.Text))
        {
            return false;
        }

        // Verificar si la imagen está seleccionada
        if (photo == null)
        {
            return false;
        }

        return true;
    }
    private void UpdateLocationState()
    {
        btnGuardar.IsEnabled = AreAllFieldsValid();
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
        UpdateLocationState();
    }




    private void btnLista_Clicked(object sender, EventArgs e)
    {
        // Lógica para manejar el evento del botón Lista
        Navigation.PushAsync(new placeList());
    }



    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string Descripcion = txtDescrip.Text;

        if (string.IsNullOrEmpty(Descripcion))
        {
            await DisplayAlert("Error", "Porfavor ingrese el nombre del autor", "OK");
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

    private void txtDescrip_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLocationState(); // Actualiza el estado del botón Guardar cuando cambia el texto
    }

    private void txtLatitud_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLocationState(); // Actualiza el estado del botón Guardar cuando cambia el texto
    }

    private void txtLongitud_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLocationState(); // Actualiza el estado del botón Guardar cuando cambia el texto
    }



    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        // Termina la aplicación
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}