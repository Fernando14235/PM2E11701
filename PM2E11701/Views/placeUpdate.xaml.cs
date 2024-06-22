using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.ObjectModel;
using PM2E11701.Views;

namespace PM2E11701.Views;

public partial class placeUpdate : ContentPage
{

    private const string GoogleMapsApiKey = "AIzaSyBn2tnAFfuslQLUfqsqKmvQsZVFbv_CutU";
    Controllers.PlaceController controller;
    FileResult photo; // Para tomar foto
    List<Models.Place> lugares;
    private int autorId;
    private string? currentPhotoBase64; // Almacenar la imagen actual en Base64
    public placeUpdate(int authorID)
	{
		InitializeComponent();

        this.autorId = authorID;
        controller = new Controllers.PlaceController();
    }

    public placeUpdate(Controllers.PlaceController dbPath)
    {
        InitializeComponent();
        controller = dbPath;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        lugares = await controller.getListPlace();
        await BuscarAutor(autorId); // Llamar a BuscarAutor aquí en lugar del constructor
    }

    private async Task BuscarAutor(int authorId)
    {
        var results = lugares
            .Where(lu => lu.Id == authorId)
            .ToList();

        if (results.Any())
        {
            var lu = results.First();

            imgFoto.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(lu.Foto)));
            currentPhotoBase64 = lu.Foto; // Almacenar la foto actual en Base64
            txtDescrip.Text = lu.Descripcion;
            txtLongitud.Text = lu.Longitud;
            txtLatitud.Text = lu.Latitud;
        }
        else
        {
            await DisplayAlert("Error", "Id no encontrado", "OK");
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
        return currentPhotoBase64; // Devolver la foto actual si no se tomó una nueva
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
        return currentPhotoBase64 != null ? Convert.FromBase64String(currentPhotoBase64) : null;
    }

    private void btnSitios_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new placeList());
    }

    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new placeList());
    }

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourcephoto = await photo.OpenReadAsync();
            using FileStream streamlocal = File.OpenWrite(photoPath);

            imgFoto.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result); // Para verla dentro de archivo

            await sourcephoto.CopyToAsync(streamlocal); // Para guardarla localmente

            // Actualizar la foto actual en Base64
            using (MemoryStream ms = new MemoryStream())
            {
                sourcephoto.Position = 0; // Reiniciar el stream
                await sourcephoto.CopyToAsync(ms);
                byte[] data = ms.ToArray();
                currentPhotoBase64 = Convert.ToBase64String(data);
            }
        }
    }

    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string Descripcion = txtDescrip.Text;

        if (string.IsNullOrEmpty(Descripcion))
        {
            await DisplayAlert("Error", "Por favor ingrese el nombre del autor", "OK");
            return;
        }

        var autor = new Models.Place
        {
            Id = autorId,
            Longitud = txtLongitud.Text,
            Latitud = txtLatitud.Text,
            Descripcion = txtDescrip.Text,
            Foto = GetImg64() // Usar la foto actual o la nueva foto en Base64
        };

        try
        {
            if (controller != null)
            {
                if (await controller.storePlace(autor) > 0)
                {
                    await DisplayAlert("Aviso", "Registro actualizado con éxito!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrió un error", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
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
}



