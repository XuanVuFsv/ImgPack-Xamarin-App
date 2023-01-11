using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PORO.Droid.Services;
using PORO.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(DatabaseConnection))]
namespace PORO.Droid.Services
{
    public class DatabaseConnection : IDatabaseConnection
    {
        public SQLiteConnection GetConnection()
        {
            var filename = "Poro.db";
            var documentspath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentspath, filename);
            var connection = new SQLite.SQLiteConnection(path);
            return connection;
        }
    }
}