using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Services
{
    public class ApiUrl
    {
        public static string UploadPhoto()
        {
            return "https://6395f3d990ac47c68078c88e.mockapi.io/api/poroapp/photos";
        }
        public static string ChangePhoto(string id)
        {
            return $"https://61b76ec064e4a10017d18b3a.mockapi.io/api/poro/ListData/{id}";
        }
        public static string Register()
        {
            return "https://6395f3d990ac47c68078c88e.mockapi.io/api/poroapp/users";
        }
        public static string GetUser(string id)
        {
            return $"https://6395f3d990ac47c68078c88e.mockapi.io/api/poroapp/users/{id}";
        }
        public static string UpdateUser(string id)
        {
            return $"https://6395f3d990ac47c68078c88e.mockapi.io/api/poroapp/users/{id}";
        }
    }
}
