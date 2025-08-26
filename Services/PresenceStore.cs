using System.Text.Json;
using System.Text.Json.Serialization;
using AttendenceProgram.Models;


namespace AttendenceProgram.Services
{
    public class PresenceStore
    {
        private readonly string _file;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _opt = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private PresenceData _data;

        public PresenceStore(IConfiguration cfg)
        {
            _file = cfg["PRESENCE_FILE"] ?? Path.Combine(AppContext.BaseDirectory, "data", "presence.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);

            if (File.Exists(_file))
            {
                var json = File.ReadAllText(_file);
                _data = JsonSerializer.Deserialize<PresenceData>(json) ?? PresenceData.CreateSkeleton();
            }
            else
            {
                _data = PresenceData.CreateSkeleton();
                Save();
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

            
            foreach (var day in r.Days.Distinct())
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
                File.WriteAllText(_file, JsonSerializer.Serialize(_data, _opt));
            }
        }

    }
}

