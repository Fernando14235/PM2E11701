using PM2E11701.Controllers;
using PM2E11701.Models;
using System.Collections.ObjectModel;

namespace PM2E11701.Views;

public partial class placeList : ContentPage
{

    private Controllers.PlaceController PlaceController;
    private List<Models.Place> autores;
    Models.Place selectedAuthor;
    private PlaceController controller;
    public ObservableCollection<Place> Autores { get; set; }
    public Command<Place> UpdateCommand { get; }
    public Command<Place> DeleteCommand { get; }


    public placeList()
	{
		InitializeComponent();

        PlaceController = new Controllers.PlaceController();
        controller = new PlaceController();
        Autores = new ObservableCollection<Place>();
        BindingContext = this;
    }

    //Metodo que permite mostrar la lista mientras la pagina se esta mostrando o cargando
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Obtiene la lista de personas de la base de datos
        autores = await PlaceController.getListPlace();

        // Coloca la lista en el collection view
        collectionView.ItemsSource = autores;
    }

    private void searchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        BuscarLugares(searchBar.Text);
    }

    private void BuscarLugares(string query)
    {

        //Usa LINQ (Language-Integrated Query) (es una característica en el framework .NET que proporciona una sintaxis estandarizada
        //y declarativa para consultar y manipular datos de diferentes tipos de fuentes, como colecciones,
        //bases de datos, XML, entre otros.) en una expresion tipo lambda para filtrar la informacion de la base 
        //de datos y mostrar los resultados basados en la busqueda.

        var results = autores
            .Where(author => author.Descripcion?.ToLowerInvariant().Contains(query.ToLowerInvariant()) == true ||
                             author.Latitud?.ToLowerInvariant().Contains(query.ToLowerInvariant()) == true)
            .ToList();

        collectionView.ItemsSource = new List<Models.Place>(results);
    }

    private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            collectionView.ItemsSource = autores;
        }
    }

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void EliminarLugar_Clicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Confirmar", "¿Está seguro que desea eliminar este sitio?", "Sí", "No");

        if (selectedAuthor != null)
        {
            if (result)
            {
                await controller.deletePlace(selectedAuthor.Id);
                Autores.Remove(selectedAuthor);

                Navigation.PopAsync();
            }
            else
            {
                return;
            }
        }
        else
        {
            await DisplayAlert("Error", "Seleccione un autor primero", "OK");
        }
    }




}