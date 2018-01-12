using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MazeApplication.Models
{
    public class Player
    {
        public int Id { get; set; }

        public string CharName { get; set; }

        public int Score { get; set; }

        public string VisitedLocations { get; set; }
        public string Inventory { get; set; }
    }
}