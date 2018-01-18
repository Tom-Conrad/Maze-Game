using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MazeApplication.Models;

namespace MazeApplication.Controllers
{
    public class MazeController : Controller
    {
        private ApplicationDbContext _dbContext;

        public MazeController()
        {
            _dbContext = new ApplicationDbContext();
        }
        
        // Methods

        public Player CalculateLocation(string id, string previousLocation)
        {
            int idAsInt = int.Parse(id);

            var player = _dbContext.Players.SingleOrDefault(p => p.Id == idAsInt);

            if (player == null)
            {
                return null;
            }

            if (previousLocation != null && player.VisitedLocations.Contains("," + previousLocation + ",") == false)
            {
                player.VisitedLocations = player.VisitedLocations + previousLocation + ",";
                _dbContext.SaveChanges();
            }

            return player;
        }

        // Pregame

        public ActionResult Index()
        {
            var players = _dbContext.Players.ToList();
            return View(players);
        }

        public ActionResult CharGeneration()
        {
            return View();
        }

        // Intermediates

        public ActionResult Maze(string charName, string inputRoom, string previousLocation, string id)
        {
            if (id == null)
            {
                _dbContext.Players.Add(new Player() { CharName = charName, VisitedLocations = ",", Inventory = ",", Score = 0 });
                _dbContext.SaveChanges();
                id = _dbContext.Players.ToList().Last().Id.ToString();
            }

            var player = CalculateLocation(id, previousLocation);

            if (player == null)
            {
                return HttpNotFound();
            }

            Room room = new Room();
            room.AccessibleLocations = new List<Location>();

            room.Name = inputRoom;
            
            switch (inputRoom)
            {
                case "Entrance":
                    room.Header = "Entrance";
                    room.Description = "You see a fork in the road.";
                    room.AccessibleLocations.Add(new Location() { Name = "LeftFork", Title = "Turn Left", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "RightFork", Title = "Turn Right", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Exit", Title = "Leave this Place", Type = "Finish" });
                    break;

                case "LeftFork":
                    room.Header = "Left Fork";
                    room.Description = "The road splits, one branch leading to a hill, the other to a valley.";
                    room.AccessibleLocations.Add(new Location() { Name = "Hill", Title = "Go to the Hill", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Valley", Title = "Go to the Valley", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Entrance", Title = "Go Back", Type = "Maze" });
                    break;

                case "RightFork":
                    room.Header = "Right Fork";
                    room.Description = "You see a ladder by the side of the road, leading up into the sky.";
                    room.AccessibleLocations.Add(new Location() { Name = "MysticalShack", Title = "Follow the Path", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Ladder", Title = "Climb the Ladder", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Entrance", Title = "Go Back", Type = "Maze" });
                    break;

                case "MysticalShack":
                    room.Header = "Teleport Shack";
                    room.Description = "You see a small shack on the side of the road. It has sign on the door, which says, \"This is a teleporter. It will take you somewhere.\"";
                    room.AccessibleLocations.Add(new Location() { Name = "LogicOrRasin", Title = "Follow the Path", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Disentigrator", Title = "Enter the Shack", Type = "Finish" });
                    room.AccessibleLocations.Add(new Location() { Name = "RightFork", Title = "Go Back", Type = "Maze" });
                    break;

                case "LogicOrRasin":
                    room.Header = "Logic or Rasin";
                    room.Description = "You a fork in the road. There is a sign at the fork. It has an arrow pointing left with the word, \"logic\" written above it. It has an arrow pointing right with the word, \"rasin\" written on it.";
                    room.AccessibleLocations.Add(new Location() { Name = "Logic", Title = "Head for Logic", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Rasin", Title = "Head for Rasin", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "MysticalShack", Title = "Go Back", Type = "Maze" });
                    break;

                case "Logic":
                    room.Header = "Logic";
                    room.Description = "There is yet another fork in the road. There are two people standing nearby, one at each fork. They both hold signs reading, \"this path leads to death\".";
                    room.AccessibleLocations.Add(new Location() { Name = "BeatenToDeath", Title = "Go Left", Type = "Finish" });
                    room.AccessibleLocations.Add(new Location() { Name = "BeatenToDeath", Title = "Go Right", Type = "Finish" });
                    room.AccessibleLocations.Add(new Location() { Name = "LogicOrRasin", Title = "Go Back", Type = "Maze" });
                    break;

                case "Rasin":
                    room.Header = "Rasin";
                    if (player.Inventory.Contains("," + "Rasin" + ","))
                    {
                        room.Description = "There is a small plinth next to the path. It is empty.";
                        room.AccessibleLocations.Add(new Location() { Name = "ENTERLATER", Title = "Follow the Path", Type = "Maze" });
                        room.AccessibleLocations.Add(new Location() { Name = "LogicOrRasin", Title = "Go Back", Type = "Maze" });
                    }
                    else
	                {
                        room.Description = "There is a small plinth next to the path. On top of it is an ornate box, with a very small rasin in it. There is a card in the box. It says, \"Behold the great rasin of truth! Hold it and ye shall see that which is hidden.\"";
                        room.AccessibleLocations.Add(new Location() { Name = "ENTERLATER", Title = "Follow the Path", Type = "Maze" });
                        room.AccessibleLocations.Add(new Location() { Name = "GetRasin", Title = "Take the Rasin", Type = "Maze" });
                        room.AccessibleLocations.Add(new Location() { Name = "LogicOrRasin", Title = "Go Back", Type = "Maze" });
                    }
                    break;

                case "GetRasin":
                    room.Description = "You pick up the rasin, and put it in your pocket. Nothing appears to happen.";
                    room.AccessibleLocations.Add(new Location() { Name = "Rasin", Title = "Continue", Type = "Maze" });
                    room.InventoryModifier = "Rasin";
                    break;

                case "Ladder":
                    room.Header = "Ladder";
                    room.Description = "You are on a ladder.";
                    room.AccessibleLocations.Add(new Location() { Name = "SkyDoor", Title = "Climb Up", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "RightFork", Title = "Climb Down", Type = "Maze" });
                    break;

                case "SkyDoor":
                    room.Header = "Sky Door";
                    room.Description = "There is a door at the top of the ladder. It does not appear to be attached to anything.";
                    room.AccessibleLocations.Add(new Location() { Name = "DoorInterim", Title = "Go Through the Door", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Ladder", Title = "Climb Down", Type = "Maze" });
                    break;

                case "DoorInterim":
                    string newLocation = "";
                    Random locationRandomizer = new Random();
                    int locationId = locationRandomizer.Next(1, 4);
                    switch (locationId)
                    {
                        case 1:
                            newLocation = "SkyDoor";
                            break;
                        case 2:
                            newLocation = "Entrance";
                            break;
                        case 3:
                            newLocation = "Dungeon";
                            break;
                    }
                    room.Description = "You find yourself somewhere else.";
                    room.AccessibleLocations.Add(new Location() { Name = newLocation, Title = "Find Out Where You Are", Type = "Maze" });
                    break;

                case "Valley":
                    room.Header = "Valley";
                    if (player.Inventory.Contains("," + "Rasin" + ","))
                    {
                        room.Description = "There is a metal hatch in the ground. It seems to be oddly out of focus.";

                    }
                    else
                    {
                        room.Description = "It's a valley";
                        room.AccessibleLocations.Add(new Location() { Name = "LeftFork", Title = "Get Bored and Leave", Type = "Maze" });
                    }
                    break;

                case "Hill":
                    room.Header = "Hill";
                    room.Description = "There is a well at the top of the hill. A rope leads down it.";
                    room.AccessibleLocations.Add(new Location() { Name = "HouseExterior", Title = "Follow the Path", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Well", Title = "Climb Down the Rope", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "LeftFork", Title = "Go Back", Type = "Maze" });
                    break;

                case "Well":
                    room.Header = "Well";
                    room.Description = "You hold on to the rope directly above the water.";
                    room.AccessibleLocations.Add(new Location() { Name = "WellUnderwater", Title = "Swim Down", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Hill", Title = "Climb Up", Type = "Maze" });
                    break;

                case "WellUnderwater":
                    room.Header = "Well Underwater";
                    room.Description = "You are underwater.";
                    if (player.Inventory.Contains("rebreather"))
                    {
                        room.AccessibleLocations.Add(new Location() { Name = "UnderwaterCave", Title = "Swim Down", Type = "Maze" });
                        room.AccessibleLocations.Add(new Location() { Name = "Well", Title = "Climb Up", Type = "Maze" });
                        ViewBag.SpecialText = "Your rebreather allows you to avoid drowning.";
                    }
                    else
                    {
                        room.AccessibleLocations.Add(new Location() { Name = "Drown", Title = "Swim Down", Type = "Finish", RequireConfirmation = true });
                        room.AccessibleLocations.Add(new Location() { Name = "Well", Title = "Climb Up", Type = "Maze" });
                        ViewBag.SpecialText = "If you continue to swim downward, you will drown.";
                    }
                    room.ScoreDisplay = "You see a bucket at the end of the rope. There is a gold coin in it.";
                    room.ScoreModifier = 2;
                    break;

                case "HouseExterior":
                    room.Header = "House Exterior";
                    room.Description = "You find an abandoned house. The door is locked.";
                    if (player.Inventory.Contains("lockpick"))
                    {
                        room.AccessibleLocations.Add(new Location() { Name = "HouseFoyer", Title = "Pick the Lock", Type = "Maze" });
                    }
                    room.AccessibleLocations.Add(new Location() { Name = "TopWindow", Title = "Climb in Through the Window", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "Hill", Title = "Go Back", Type = "Maze" });
                    break;

                case "TopWindow":
                    room.Header = "Top Room";
                    room.Description = "You climb into a top floor room through a window. There does not appear to be much in the room. There is a door leading somewehere else.";
                    room.AccessibleLocations.Add(new Location() { Name = "Hallway", Title = "Go Through the Door", Type = "Maze" });
                    room.AccessibleLocations.Add(new Location() { Name = "HouseExterior", Title = "Return to the Ground", Type = "Maze" });
                    break;

                case "Hallway":
                    room.Header = "Hallway";
                    room.Description = "You walk out into the hallway. You hear a beeping noise, and see a turret at the end of the hallway.";
                    room.AccessibleLocations.Add(new Location() { Name = "Turret", Title = "Run past the turret", Type = "Finish" });
                    room.AccessibleLocations.Add(new Location() { Name = "TopWindow", Title = "Go Back", Type = "Maze" });
                    break;

                default:
                    room.Description = "You are nowhere. There's probably been a mistake of some sort.";
                    break;
            }

            if (room.Header != null && player.VisitedLocations.Contains("," + room.Header + ",") == true)
            {
                room.AlreadySeen = true;
            }

            if (room.ScoreModifier != 0 && room.AlreadySeen == false)
            {
                player.Score += room.ScoreModifier;
                _dbContext.SaveChanges();
            }

            if (room.InventoryModifier != null && room.AlreadySeen == false)
            {
                player.Inventory += room.InventoryModifier + ",";
                _dbContext.SaveChanges();
            }

            ViewBag.Id = player.Id;
            ModelState.Clear();
            return View(room);
        }

        public ActionResult Finish(string previousLocation, string inputRoom, string id)
        {
            var player = CalculateLocation(id, previousLocation);

            if (player == null)
            {
                return HttpNotFound();
            }

            FinishRoom finishRoom = new FinishRoom();
            finishRoom.Name = inputRoom;
            finishRoom.Score = player.Score;

            switch (inputRoom)
            {
                case "Exit":
                    finishRoom.Header = "Leave";
                    break;

                case "Drown":
                    finishRoom.Header = "You drown.";
                    finishRoom.Score = 0;
                    break;

                case "Turret":
                    finishRoom.Header = "The turret shoots you. You die.";
                    finishRoom.Score = 0;
                    break;

                case "Disintegrator":
                    finishRoom.Header = "You enter the shack. It is empty. There is a blinding flash of light. You die.";
                    finishRoom.Score = 0;
                    break;

                case "BeatenToDeath":
                    finishRoom.Header = "You start to walk down the road, and the person standing next to it beats you to death with the sign.";
                    finishRoom.Score = 0;
                    break;

                default:
                    finishRoom.Header = "You are nowhere. There's probably been a mistake of some sort.";
                    finishRoom.Score = 0;
                    break;
            }

            player.Score = finishRoom.Score;
            _dbContext.SaveChanges();

            return View(finishRoom);
        }

        public ActionResult Danger(string type, string inputRoom, string previousLocation, string previousName, string id)
        {
            var player = CalculateLocation(id, previousLocation);

            if (player == null)
            {
                return HttpNotFound();
            }

            Danger danger = new Danger() { Type = type, InputRoom = inputRoom, PreviousName = previousName, Id = id };
            ModelState.Clear();
            return View(danger);
        }

        public ActionResult Menu(string previousLocation, string currentRoom, string menuType, string id)
        {
            var player = CalculateLocation(id, previousLocation);

            if (player == null)
            {
                return HttpNotFound();
            }

            Menu menu = new Menu();
            menu.CurrentLocation = currentRoom;
            menu.Player = player;
            menu.MenuType = menuType;

            ModelState.Clear();
            return View(menu);
        }
    }
}