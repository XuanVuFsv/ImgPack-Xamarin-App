using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Interfaces
{
    public interface IDatabaseConnection
    {
        SQLiteConnection GetConnection();
    }
}
