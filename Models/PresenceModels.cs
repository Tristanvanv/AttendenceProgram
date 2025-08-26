using System;
using System.Collections.Generic;

namespace AttendenceProgram.Models
{
   
    public class PresenceSetRequest
    {
        public string HouseholdId { get; set; } = "";
        public string PersonId { get; set; } = "";
        public int WeekOffset { get; set; }      
        public List<int> Days { get; set; } = new List<int>(); 
    }

    public class Household
    {
        public string name { get; set; } = "";
        public string? color { get; set; } 
    }

    public class Person
    {
        public string name { get; set; } = "";
    }

    
    public class Schedule
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> byOffset { get; set; }
            = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
    }

    public class PresenceData
    {
        public Dictionary<string, Household> households { get; set; }
            = new Dictionary<string, Household>();

        public Dictionary<string, Person> people { get; set; }
            = new Dictionary<string, Person>();

        public Schedule schedule { get; set; } = new Schedule();

        public string? lastUpdated { get; set; }
        public int version { get; set; }

        
        public static PresenceData CreateSkeleton()
        {
            var d = new PresenceData();

            d.households["yvonne"] = new Household { name = "Yvonne", color = "#d8d262" };
            d.households["elly"] = new Household { name = "Elly", color = "#d8d262" };

            d.people["tristan"] = new Person { name = "Tristan" };
            d.people["martine"] = new Person { name = "Martine" };

            
            d.schedule.byOffset["0"] = new Dictionary<string, Dictionary<string, List<string>>>();
            d.schedule.byOffset["1"] = new Dictionary<string, Dictionary<string, List<string>>>();

            return d;
        }
    }
}