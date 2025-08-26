using System.Text.Json;
using System.Text.Json.Serialization;
using AttendenceProgram.Models;
using Npgsql;


namespace AttendenceProgram.Services
{
    public class PresenceStore
    {
        private readonly string _file;
        private readonly object _lock = new();
        private readonly string? _conn;
        private readonly bool _useDb;
        private readonly JsonSerializerOptions _opt = new()
        
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private PresenceData _data = new PresenceData();

        public PresenceStore(IConfiguration cfg)
        {

            _conn = cfg["CONNECTION_STRING"];
            _useDb = !string.IsNullOrWhiteSpace(_conn);

            _file = cfg["PRESENCE_FILE"] ?? Path.Combine(AppContext.BaseDirectory, "data", "presence.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);

            if (_useDb)
            {
                EnsureTable();
                var loaded = DbLoad();
                if (loaded != null)
                {
                    _data = loaded;
                }
                else
                {
                    _data = PresenceData.CreateSkeleton();
                    DbSave(); // eerste seed
                }
            }
            else
            {
                if (File.Exists(_file))
                {
                    var json = File.ReadAllText(_file);
                    _data = JsonSerializer.Deserialize<PresenceData>(json) ?? PresenceData.CreateSkeleton();
                }
                else
                {
                    _data = PresenceData.CreateSkeleton();
                    FileSave();
                }
            }
        }

        public PresenceData GetAll() => _data;

        public bool Set(PresenceSetRequest r, out string? error)
        {
            error = null;

            if (r.WeekOffset is < 0 or > 1) { error = "weekOffset must be 0 or 1"; return false; }
            if (r.Days.Any(d => d < 1 || d > 7)) { error = "days must be 1..7"; return false; }
            if (!_data.households.ContainsKey(r.HouseholdId)) { error = "unknown householdId"; return false; }
            if (!_data.people.ContainsKey(r.PersonId)) { error = "unknown personId"; return false; }

            var o = r.WeekOffset.ToString();
            _data.schedule.byOffset.TryAdd(o, new());

            
            for (var day = 1; day <= 7; day++)
            {
                var dk = day.ToString();
                _data.schedule.byOffset.TryAdd(o, new());
                _data.schedule.byOffset[o].TryAdd(dk, new());
                _data.schedule.byOffset[o][dk].TryAdd(r.HouseholdId, new());
                _data.schedule.byOffset[o][dk][r.HouseholdId].RemoveAll(id => id == r.PersonId);
            }

           
            foreach (var day in r.Days.Distinct().OrderBy(d => d))
            {
                var dk = day.ToString();
                _data.schedule.byOffset[o].TryAdd(dk, new());
                _data.schedule.byOffset[o][dk].TryAdd(r.HouseholdId, new());
                var list = _data.schedule.byOffset[o][dk][r.HouseholdId];
                if (!list.Contains(r.PersonId)) list.Add(r.PersonId);
            }

            Save();
            return true;
        }

       

        private void Save()
        {
            lock (_lock)
            {
                _data.lastUpdated = DateTime.UtcNow.ToString("o");
                _data.version = _data.version == 0 ? 1 : _data.version + 1;

                if (_useDb) DbSave();
                else FileSave();
            }
        }

        private void FileSave()
        {
            File.WriteAllText(_file, JsonSerializer.Serialize(_data, _opt));
        }

        private void EnsureTable()
        {
            using var conn = new NpgsqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                create table if not exists presence_state(
                  id int primary key,
                  json jsonb not null,
                  last_updated timestamptz not null default now(),
                  version int not null default 1
                );";
            cmd.ExecuteNonQuery();
        }

        private PresenceData? DbLoad()
        {
            using var conn = new NpgsqlConnection(_conn);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "select json, version, last_updated from presence_state where id = 1;";
            using var rdr = cmd.ExecuteReader();

            if (!rdr.Read()) return null;

            var json = rdr.GetFieldValue<string>(0);
            var pd = JsonSerializer.Deserialize<PresenceData>(json) ?? new PresenceData();

           
            if (!rdr.IsDBNull(1)) pd.version = rdr.GetInt32(1);
            if (!rdr.IsDBNull(2)) pd.lastUpdated = rdr.GetDateTime(2).ToUniversalTime().ToString("o");

            
            pd.schedule.byOffset.TryAdd("0", new Dictionary<string, Dictionary<string, List<string>>>());
            pd.schedule.byOffset.TryAdd("1", new Dictionary<string, Dictionary<string, List<string>>>());

            return pd;
        }

        private void DbSave()
        {
            using var conn = new NpgsqlConnection(_conn);
            conn.Open();

            
            var json = JsonSerializer.Serialize(_data, _opt);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                insert into presence_state (id, json, last_updated, version)
                values (1, cast(@json as jsonb), now(), 1)
                on conflict (id) do update
                  set json = excluded.json,
                      last_updated = now(),
                      version = presence_state.version + 1
                returning version, last_updated;";
            cmd.Parameters.AddWithValue("@json", json);

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                _data.version = rdr.GetInt32(0);
                _data.lastUpdated = rdr.GetDateTime(1).ToUniversalTime().ToString("o");
            }
        }
    }
}

