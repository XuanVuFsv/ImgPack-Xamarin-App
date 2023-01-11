using PORO.Interfaces;
using PORO.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace PORO.Services.Database
{
    public class Database
    {
        static object locker = new object();
        SQLiteConnection _sqlconnection;

        public Database()
        {
            _sqlconnection = DependencyService.Get<IDatabaseConnection>().GetConnection();
            //_sqlconnection.CreateTable<DatabaseModel>();
            _sqlconnection.CreateTable<UserModel>();
        }
        public int Insert(UserModel user)
        {
            lock (locker)
            {
                return _sqlconnection.Insert(user);
            }
        }
        public int Update(UserModel user)
        {
            lock (locker)
            {
                return _sqlconnection.Update(user);
            }
        }
        public int Delete(string id)
        {
            lock (locker)
            {
                return _sqlconnection.Delete<UserModel>(id);
            }
        }
        public IEnumerable<UserModel> GetAll()
        {
            lock (locker)
            {
                return (from i in _sqlconnection.Table<UserModel>() select i).ToList();
            }
        }
        public int FullDelete()
        {
            lock (locker)
            {
                return _sqlconnection.DeleteAll<UserModel>();
            }
        }
        public UserModel Get(string id)
        {
            lock (locker)
            {
                return _sqlconnection.Table<UserModel>().FirstOrDefault(t => t.Id == id);
            }
        }
        //string folder = System.Environment.GetFolderPath
        //(System.Environment.SpecialFolder.Personal);
        //public bool CreateDatabase()
        //{
        //    try
        //    {
        //        string path = System.IO.Path.Combine(folder, "poro.db");
        //        var connection = new SQLiteConnection(path);
        //        connection.CreateTable<UserModel>();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public bool AddTopic(UserModel topicModel)
        //{
        //    try
        //    {
        //        string path = System.IO.Path.Combine(folder, "poro.db");
        //        var connection = new SQLiteConnection(path);
        //        connection.Insert(topicModel);
        //        return true;
        //    }
        //    catch
        //    {

        //        return false;
        //    }
        //}
        //public bool UpdateUser(UserModel topicModel)
        //{
        //    try
        //    {
        //        string path = System.IO.Path.Combine(folder, "poro.db");
        //        var connection = new SQLiteConnection(path);
        //        connection.Update(topicModel);
        //        return true;
        //    }
        //    catch
        //    {

        //        return false;
        //    }
        //}

        //public List<UserModel> GetAll()
        //{
        //    try
        //    {
        //        string path = System.IO.Path.Combine(folder, "poro.db");
        //        var connection = new SQLiteConnection(path);
        //        return connection.Table<UserModel>().ToList();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        //public List<UserModel> GetUser(string id)
        //{
        //    try
        //    {
        //        string path = System.IO.Path.Combine(folder, "poro.db");
        //        var connection = new SQLiteConnection(path);
        //        //return connection.Table<Hotel>().ToList();
        //        return connection.Query<UserModel>(
        //            "select * from UserModel where Id=" + id
        //            );
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }
}
