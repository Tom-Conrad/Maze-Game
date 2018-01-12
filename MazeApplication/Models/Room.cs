using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MazeApplication.Models
{
    public class Room
    {
        public string Name { get; set; }

        public string Header { get; set; }

        public string Description { get; set; }

        public List<Location> AccessibleLocations { get; set; }

        public int ScoreModifier { get; set; }
        
        public string ScoreDisplay { get; set; }

        public bool AlreadySeen { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public bool RequireConfirmation { get; set; }
    }

    public class Temporary
    {
        public string Type { get; set; }

        public string InputRoom { get; set; }
        
        public string PreviousLocation { get; set; }

        public string Id { get; set; }
    }
}