using System;
using LiteDB;
using System.IO;

namespace ACbr.Net.Storage
{
    public static class Connect
    {

        public static LiteDatabase OpenConnection(string path)
        {
            if (Directory.Exists(path))
            {
                return new LiteDatabase($@"{path}\SmartNotas.db");
            }
            else
            {
                throw new Exception($"O seguinte diretório não está acessível: {path}");
            }
        }

    }
}
