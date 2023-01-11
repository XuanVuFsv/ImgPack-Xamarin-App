using Foundation;
using PORO.Interfaces;
using PORO.iOS.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(DatabaseConnection))]
namespace PORO.iOS.Services
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