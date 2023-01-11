using PORO.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Models
{
    public class EditImageModel
    {
        [PrimaryKey, AutoIncrement]

        public int id { get; set; }
        public string Name { get; set; }
        public FilterType FilterTypeChosen { get; set; }
        public float ColorContrast { get; set; }
        public float ColorBrightness { get; set; }
        public float ColorWhiteBalance { get; set; }
        public float GrayContrast { get; set; }
        public float GrayBrightness { get; set; }
        public float BlackWhiteContrast { get; set; }
    }
}
