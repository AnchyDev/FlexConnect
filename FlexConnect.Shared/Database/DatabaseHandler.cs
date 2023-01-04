using FlexConnect.Shared.MasterList;
using MySqlConnector;
using System.Data.SqlClient;

namespace FlexConnect.Shared.Database
{
    public class DatabaseHandler
    {
        private MySqlConnection _mysql;

        public DatabaseHandler(string user, string pass, string db, string server)
        {
            _mysql = new MySqlConnection($"Server={server};User ID={user};Password={pass};Database={db}");
        }

        public async Task ConnectAsync()
        {
            await _mysql.OpenAsync();
        }

        public async Task<List<ServerInfo>> GetServerInfoAsync()
        {
            using var command = new MySqlCommand("SELECT * FROM realmlists;", _mysql);
            await using var reader = await command.ExecuteReaderAsync();

            var serverList = new List<ServerInfo>();

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string desc = reader.GetString(2);
                string realm = reader.GetString(3);

                serverList.Add(new ServerInfo()
                {
                    Name = name,
                    Description = desc,
                    Realmlist = realm
                });
            }

            return serverList;
        }
    }
}
