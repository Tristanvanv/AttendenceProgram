using System.Text.Json.Serialization;

namespace AttendenceProgram.Models
{
    public record PresenceModels(string HouseholdId, string PersonId, int WeekOffset, List<int> Days);

    public class Household { public string name { get; set; } = ""; public string? color { get; set; } }
    public class Person { public string name { get; set; } = ""; }

    public class Schedule
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> byOffset { get; set; }
        = new();
    }

    public class PresenceData
    {
        public Dictionary<string, Household> households { get; set; } = new();
        public Dictionary<string, Person> people { get; set; } = new();
        public Schedule schedule { get; set; } = new()
        {
            byOffset = new () { ["0"] = new (), ["1"] = new () }
        };

        public string? lastUpdated { get; set; }
        public int version { get; set; }

        public static PresenceData CreateSkeleton() => new PresenceData
        {
            households = new()
            {
                ["yvonne"] = new Household { name = "Yvonne" },
                ["elly"] = new Household { name = "Elly" },
            },
            people = new()
            {
                ["tristan"] = new Person { name = "Tristan" },
                ["martine"] = new Person { name = "Martine" },
            }
        };
    }
}
