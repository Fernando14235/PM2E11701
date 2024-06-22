using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM2E11701.Models;

namespace PM2E11701.Controllers
{
    public class PlaceController
    {
        SQLiteAsyncConnection _connection;

        public PlaceController() { }


        //Conexion a la base de datos
        public async Task Init()
        {
            try
            {
                if (_connection is null)
                {
                    SQLite.SQLiteOpenFlags extensiones = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;

                    if (string.IsNullOrEmpty(FileSystem.AppDataDirectory))
                    {
                        return;
                    }

                    _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "DBPersonas"), extensiones);

                    var creacion = await _connection.CreateTableAsync<Models.Place>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Init(): {ex.Message}");
            }
        }


        //Crear metodos crud para la clase personas
        //Create
        public async Task<int> storePlace(Place place)
        {
            await Init();
            if (place.Id == 0)
            {
                return await _connection.InsertAsync(place);
            }
            else
            {
                return await _connection.UpdateAsync(place);
            }
        }

        //Update
        public async Task<int> updatePlace(Place place)
        {
            await Init();
            return await _connection.UpdateAsync(place);
        }

        //Read
        public async Task<List<Models.Place>> getListPlace()
        {
            await Init();
            return await _connection.Table<Place>().ToListAsync();
        }

        //Read Element
        public async Task<Models.Place> getPlace(int pid)
        {
            await Init();
            return await _connection.Table<Place>().Where(i => i.Id == pid).FirstOrDefaultAsync();
        }

        //Delete
        public async Task<int> deletePlace(int placeID)
        {
            await Init();
            var placeToDelete = await getPlace(placeID);

            if (placeToDelete != null)
            {
                return await _connection.DeleteAsync(placeToDelete);
            }

            return 0;
        }
    }
}
