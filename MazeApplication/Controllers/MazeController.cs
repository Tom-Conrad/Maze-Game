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

        public Player CalculateLocation(string id, string previousLocation, string previousHeader)
        {
            int idAsInt = int.Parse(id);

            var player = _dbContext.Players.SingleOrDefault(p => p.Id == idAsInt);

            if (player == null)
            {
                return null;
            }
            
            if (previousLocation != null && player.VisitedLocations.Contains("," + previousLocation + ",") == false)
            {
                player.VisitedLocations += previousLocation + ",";
                player.VisitedHeaders += previousHeader + ",";
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

        public ActionResult Maze(string charName, string inputLocation, string previousLocation, string previousHeader, string id)
        {
            if (previousHeader == "")
            {
                previousHeader = null;
            }

            if (id == null)
            {
                _dbContext.Players.Add(new Player() { CharName = charName, VisitedLocations = ",", VisitedHeaders = ",", Inventory = ",", Score = 0});
                _dbContext.SaveChanges();
                id = _dbContext.Players.ToList().Last().Id.ToString();
            }

            var player = CalculateLocation(id, previousLocation, previousHeader);

            if (player == null)
            {
                return HttpNotFound();
            }

            if (player.CurrentLocation != previousLocation)
            {
                return RedirectToAction("Discrepancy", player);
            }

            if (player.Locked == true)
            {
                return RedirectToAction("Cheater");
            }

            Location location = new Location();
            location.AccessibleLocations = new List<Link>();

            location.Name = inputLocation;
            
            switch (inputLocation)
            {
                case "Entrance":
                    location.Header = "Entrance";
                    location.Description = "You see a fork in the road.";
                    location.AccessibleLocations.Add(new Link() { Name = "LeftFork", Title = "Turn Left", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "RightFork", Title = "Turn Right", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Exit", Title = "Leave this Place (and end the game)", Type = "Finish", RequireConfirmation = true });
                    break;

                case "LeftFork":
                    location.Header = "Left Fork";
                    location.Description = "The road splits, one branch leading to a hill, the other to a valley.";
                    location.AccessibleLocations.Add(new Link() { Name = "Hill", Title = "Go to the Hill", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Valley", Title = "Go to the Valley", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Entrance", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "RightFork":
                    location.Header = "Right Fork";
                    location.Description = "You see a ladder by the side of the road, leading up into the sky.";
                    location.AccessibleLocations.Add(new Link() { Name = "MysticalShack", Title = "Follow the Path", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Ladder", Title = "Climb the Ladder", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Entrance", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "MysticalShack":
                    location.Header = "Teleport Shack";
                    location.Description = "You see a small shack on the side of the road. It has sign on the door, which says, \"This is a teleporter. It will take you somewhere.\"";
                    location.AccessibleLocations.Add(new Link() { Name = "LogicOrRaisin", Title = "Follow the Path", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Disintegrator", Title = "Enter the Shack", Type = "Finish" });
                    location.AccessibleLocations.Add(new Link() { Name = "RightFork", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "LogicOrRaisin":
                    location.Header = "Logic or Raisin";
                    location.Description = "You see a fork in the road. There is a sign at the fork. It has an arrow pointing left with the word, \"logic\" written above it. It has an arrow pointing right with the word, \"Raisin\" written on it.";
                    location.AccessibleLocations.Add(new Link() { Name = "Logic", Title = "Head for Logic", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Raisin", Title = "Head for Raisin", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "MysticalShack", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "Logic":
                    location.Header = "Logic";
                    location.Description = "There is yet another fork in the road. There are two people standing nearby, one at each fork. They both hold signs reading, \"this path leads to death\".";
                    location.AccessibleLocations.Add(new Link() { Name = "BeatenToDeath", Title = "Go Left", Type = "Finish" });
                    location.AccessibleLocations.Add(new Link() { Name = "BeatenToDeath", Title = "Go Right", Type = "Finish" });
                    location.AccessibleLocations.Add(new Link() { Name = "LogicOrRaisin", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "Raisin":
                    location.Header = "Raisin";
                    if (player.Inventory.Contains("," + "Raisin" + ","))
                    {
                        location.Description = "There is a small plinth next to the path. It is empty.";
                        location.AccessibleLocations.Add(new Link() { Name = "MountainPath", Title = "Follow the Path", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "LogicOrRaisin", Title = "Go Back Along the Path", Type = "Maze" });
                    }
                    else
	                {
                        location.Description = "There is a small plinth next to the path. On top of it is an ornate box, with a very small raisin in it. There is a card in the box. It says, \"Behold the great raisin of truth! Hold it and ye shall see that which is hidden.\"";
                        location.AccessibleLocations.Add(new Link() { Name = "MountainPath", Title = "Follow the Path", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "GetRaisin", Title = "Take the Raisin", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "LogicOrRaisin", Title = "Go Back", Type = "Maze" });
                    }
                    break;

                case "GetRaisin":
                    location.Description = "You pick up the raisin, and put it in your pocket. Nothing appears to happen.";
                    location.AccessibleLocations.Add(new Link() { Name = "Raisin", Title = "Continue", Type = "Maze" });
                    location.InventoryModifier = "Raisin";
                    break;

                case "Ladder":
                    location.Header = "Ladder";
                    location.Description = "You are on a ladder.";
                    location.AccessibleLocations.Add(new Link() { Name = "SkyDoor", Title = "Climb Up", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "RightFork", Title = "Climb Down", Type = "Maze" });
                    break;

                case "SkyDoor":
                    location.Header = "Sky Door";
                    location.Description = "There is a door at the top of the ladder. It does not appear to be attached to anything.";
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
                            newLocation = "Cave";
                            break;
                    }
                    location.AccessibleLocations.Add(new Link() { Name = newLocation, Title = "Go Through the Door", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Ladder", Title = "Climb Down", Type = "Maze" });
                    break;

                case "Valley":
                    location.Header = "Valley";
                    if (player.Inventory.Contains("," + "Raisin" + ","))
                    {
                        location.Description = "There is a metal hatch in the ground. It seems to be oddly out of focus.";
                        location.AccessibleLocations.Add(new Link() { Name = "DungeonEntrance", Title = "Enter the Hatch", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "LeftFork", Title = "Go Back Along the Path", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "It's a valley";
                        location.AccessibleLocations.Add(new Link() { Name = "LeftFork", Title = "Get Bored and Leave", Type = "Maze" });
                    }
                    break;

                case "DungeonEntrance":
                    location.Header = "Antechamber";
                    location.Description = "You see a large room with stone walls. There are two passages leading away from the room.";
                    location.AccessibleLocations.Add(new Link() { Name = "LeftDungeonPassage", Title = "Turn Left", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "RightDungeonPassage", Title = "Turn Right", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Valley", Title = "Climb Back Aboveground", Type = "Maze" });
                    break;

                case "LeftDungeonPassage":
                    location.Header = "Left Passage";
                    location.Description = "The passage splits into two.";
                    location.AccessibleLocations.Add(new Link() { Name = "BarredDoor", Title = "Turn Left", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Chute", Title = "Turn Right", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "DungeonEntrance", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "RightDungeonPassage":
                    location.Header = "Right Passage";
                    location.Description = "The passage divides into two staircases, one leading up, and one leading down.";
                    location.AccessibleLocations.Add(new Link() { Name = "UpperDungeonLanding", Title = "Go Up", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "LowerDungeonLanding", Title = "Go Down", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "DungeonEntrance", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "LowerDungeonLanding":
                    location.Header = "Lower Landing";
                    if (player.Inventory.Contains("," + "Flashlight" + ","))
                    {
                        location.Description = "The tunnel quickly descends into darkness. You feel a draft, and realize that there is a large pit in front of you. Using your flashlight you are able to see a path across the pit.";
                        location.AccessibleLocations.Add(new Link() { Name = "Note", Title = "Attempt to Cross", Type = "Maze", RequireConfirmation = true });
                    }
                    else
                    {
                        location.Description = "The tunnel quickly descends into darkness. You feel a draft, and realize that there is a large pit in front of you. You can feel a thin bridge across it, but you doubt that you will be able to safely cross in the dark.";
                        location.AccessibleLocations.Add(new Link() { Name = "FallToDeath", Title = "Attempt to Cross", Type = "Finish", RequireConfirmation = true });
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "RightDungeonPassage", Title = "Go Back Up", Type = "Maze" });
                    break;

                case "Note":
                    location.Header = "Far Side of the Pit";
                    location.Description = "You enter a small cavelike room. There are no exits apart from the way you came in.";
                    location.AccessibleLocations.Add(new Link() { Name = "LowerDungeonLanding", Title = "Go Back Across the Path", Type = "Maze" });
                    location.InventoryModifier = "Combination";
                    location.InventoryDisplay = "You see a small note on the ground. There is a series of numbers written on it.";
                    break;

                case "UpperDungeonLanding":
                    location.Header = "Upper Landing";
                    location.Description = "You see a rather plain passage.";
                    location.AccessibleLocations.Add(new Link() { Name = "Branches", Title = "Go Forward Through the Passage", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "RightDungeonPassage", Title = "Go Back Down", Type = "Maze" });
                    location.InventoryModifier = "Axe";
                    location.InventoryDisplay = "You notice an axe on the ground, with a note next to it. The note says, \"You might want this for the next room.\"";
                    break;

                case "Branches":
                    location.Header = "Entangled Mass";
                    if (player.VisitedHeaders.Contains("," + "Forest" + ","))
                    {
                        location.Description = "You see a large mass of entangled greenery with a path cleared through the center.";
                        location.AccessibleLocations.Add(new Link() { Name = "Forest", Title = "Go Forward Through the Greenery", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "You see a large mass of entangled greenery blocking the path forward.";
                        location.AccessibleLocations.Add(new Link() { Name = "Forest", Title = "Cut a Path Through the Greenery", Type = "Maze" });
                        
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "UpperDungeonLanding", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "Forest":
                    location.Header = "Forest";
                    if (previousHeader == "Forest")
                    {
                        location.Description = "You become hoplessly lost, only barely managing to navigate back to your original starting point.";
                    }
                    else
                    {
                        location.Description = "You see a large forest ahead of you. It is brightly lit from no obvious source. There is a cave ceiling high above";
                    }
                    if (player.Inventory.Contains("," + "Map" + ","))
                    {
                        location.Description = "You see a large forest ahead of you. It seems to have a very similar layout to the forest in your map.";
                        location.AccessibleLocations.Add(new Link() { Name = "Workshop", Title = "Traverse the Forest", Type = "Maze" });
                    }
                    else
                    {
                        location.AccessibleLocations.Add(new Link() { Name = "Forest", Title = "Attempt to Navigate Through the Forest", Type = "Maze" });
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "Branches", Title = "Go Back Through the Greenery", Type = "Maze" });
                    break;

                case "Workshop":
                    location.Header = "Workshop";
                    location.Description = "After a long travel, you manage to find the building depicted in your map. It is filled with mechanical parts and tools.";
                    location.InventoryDisplay = "You notice a large and very useful seeming toolkit.";
                    location.InventoryModifier = "Toolkit";
                    location.AccessibleLocations.Add(new Link() { Name = "Forest", Title = "Return to the Forest", Type = "Maze" });
                    break;

                case "Chute":
                    location.Header = "Hole in the Floor";
                    location.Description = "You see a dark hole in the ground in front of you. You have no way of knowing how deep it is.";
                    location.AccessibleLocations.Add(new Link() { Name = "Cave", Title = "Jump Down the Hole", Type = "Maze", RequireConfirmation = true });
                    location.AccessibleLocations.Add(new Link() { Name = "LeftDungeonPassage", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "Cave":
                    location.Header = "Cave";
                    location.Description = "You find yourself in a dark cave. You hear the sound of running water in the distance.";
                    location.AccessibleLocations.Add(new Link() { Name = "CaveRiver", Title = "Head Toward the Noise", Type = "Maze" });
                    break;

                case "CaveRiver":
                    location.Header = "Underground River";
                    location.Description = "You see a river in the cave. There is a faint light at the end of the river.";
                    location.AccessibleLocations.Add(new Link() { Name = "MountainRiver", Title = "Swim Toward the Light", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Cave", Title = "Go Farther Into the Cave.", Type = "Maze"});
                    location.InventoryModifier = "Rebreather";
                    location.InventoryDisplay = "You notice a rebreather on the ground. It might help you get out of this cave.";
                    break;

                case "MountainRiver":
                    location.Header = "Mountain River";
                    location.Description = "You are on top of a mountain. There is a river here, leading into a cave.";
                    location.AccessibleLocations.Add(new Link() { Name = "MountainPath", Title = "Climb Down the Mountain", Type = "Maze" });
                    if (player.Inventory.Contains("," + "Rebreather" + ","))
                    {
                        location.AccessibleLocations.Add(new Link() { Name = "CaveRiver", Title = "Swim Into the Cave.", Type = "Maze" });
                    }
                    break;

                case "MountainPath":
                    location.Header = "Path Next to Mountain";
                    location.Description = "You are on a path. There is a mountain nearby.";
                    location.AccessibleLocations.Add(new Link() { Name = "MountainRiver", Title = "Climb Up the Mountain", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Raisin", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "BarredDoor":
                    location.Header = "Door With Skeleton";
                    if (player.VisitedHeaders.Contains("," + "Treasury" + ","))
                    {
                        location.Description = "You see a door. It has been broken through. There is a skeleton on the floor next to it.";
                        location.AccessibleLocations.Add(new Link() { Name = "GoldRoom", Title = "Enter the Room", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "You see a door. It is firmly barred. There is a skeleton leaned aginst the door.";
                        location.InventoryModifier = "Lockpick";
                        location.InventoryDisplay = "You see a lockpick in the skeleton's hand. You grab it, thinking that it might prove useful.";
                        if (player.Inventory.Contains("," + "Axe" + ","))
                        {
                            location.AccessibleLocations.Add(new Link() { Name = "GoldRoom", Title = "Hack Through the Door With the Axe", Type = "Maze" });
                        }
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "LeftDungeonPassage", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "GoldRoom":
                    location.Header = "Treasury";
                    location.Description = "This room is filled with gold coins";
                    location.ScoreDisplay = "You grab as much treasure as you can carry.";
                    location.ScoreModifier = 10;
                    location.AccessibleLocations.Add(new Link() { Name = "BarredDoor", Title = "Go Back Through the Passage", Type = "Maze" });
                    break;

                case "Hill":
                    location.Header = "Hill";
                    location.Description = "There is a well at the top of the hill. A rope leads down it.";
                    location.AccessibleLocations.Add(new Link() { Name = "HouseExterior", Title = "Follow the Path", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Well", Title = "Climb Down the Rope", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "LeftFork", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "Well":
                    location.Header = "Well";
                    location.Description = "You hold on to the rope directly above the water.";
                    location.AccessibleLocations.Add(new Link() { Name = "WellUnderwater", Title = "Swim Down", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Hill", Title = "Climb Up", Type = "Maze" });
                    break;

                case "WellUnderwater":
                    location.Header = "Well Underwater";
                    
                    if (player.Inventory.Contains("Rebreather"))
                    {
                        location.AccessibleLocations.Add(new Link() { Name = "SunkenShip", Title = "Swim Down", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "Well", Title = "Climb Up", Type = "Maze" });
                        location.Description = "You are underwater. Your rebreather allows you to avoid drowning.";
                    }
                    else
                    {
                        location.AccessibleLocations.Add(new Link() { Name = "Drown", Title = "Swim Down", Type = "Finish", RequireConfirmation = true });
                        location.AccessibleLocations.Add(new Link() { Name = "Well", Title = "Climb Up", Type = "Maze" });
                        location.Description = "You are underwater. If you continue to swim downward, you will drown.";
                    }
                    location.ScoreDisplay = "You see a bucket at the end of the rope. There is a gold coin in it.";
                    location.ScoreModifier = 2;
                    break;

                case "SunkenShip":
                    location.Header = "Sunken Ship";
                    location.Description = "You find the sunken remains of a ship on the ocean floor.";
                    location.AccessibleLocations.Add(new Link() { Name = "ShipInterior", Title = "Enter the Ship", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "MazeEntrance", Title = "Explore the Ocean Floor", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "WellUnderwater", Title = "Swim Back Up", Type = "Maze" });
                    location.InventoryDisplay = "You notice a corpse tangled in the rigging. It has a flashlight in its grip, which, as it turns out, works underwater.";
                    location.InventoryModifier = "Flashlight";
                    break;

                case "ShipInterior":
                    location.Header = "Ship Interior";
                    location.Description = "You look around the interior of the ship, finding little of value.";
                    location.AccessibleLocations.Add(new Link() { Name = "SunkenShip", Title = "Leave the Ship", Type = "Maze" });
                    location.InventoryDisplay = "You are able to locate a small sealed container. You find a small pocket of air, and open the contianer. It appears to be a map of a forest, with some type of building at one end.";
                    location.InventoryModifier = "Map";
                    break;

                case "HouseExterior":
                    location.Header = "House Exterior";
                    if (player.VisitedLocations.Contains("," + "HouseFoyer" + ","))
                    {
                        location.Description = "You see an abandoned house.";
                        location.AccessibleLocations.Add(new Link() { Name = "HouseFoyer", Title = "Enter the House", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "You see an abandoned house. The door is locked.";
                        if (player.Inventory.Contains("Lockpick"))
                        {
                            location.AccessibleLocations.Add(new Link() { Name = "HouseFoyer", Title = "Pick the Lock", Type = "Maze" });
                        }
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "TopWindow", Title = "Climb in Through the Window", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Hill", Title = "Go Back Along the Path", Type = "Maze" });
                    break;

                case "HouseFoyer":
                    location.Header = "Foyer";
                    location.Description = "You stand in a large empty room. The only obvious exit is a small staircase directly ahead.";
                    location.AccessibleLocations.Add(new Link() { Name = "ControlRoom", Title = "Climb the Stairs", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "HouseExterior", Title = "Go Back Outside", Type = "Maze" });
                    break;

                case "ControlRoom":
                    location.Header = "Control Room";
                    if (player.VisitedLocations.Contains("," + "DeactivateTurret" + ","))
                    {
                        location.Description = "You are in a small room, filled with vague electronics. There is a single switch on the wall, labled \"turret\". It is currently in the off position.";
                        location.AccessibleLocations.Add(new Link() { Name = "HouseFoyer", Title = "Return to the Foyer", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "You enter a small room, filled with vague electronics. There is a single switch on the wall, labled \"turret\". It is currently in the on position.";
                        location.AccessibleLocations.Add(new Link() { Name = "DeactivateTurret", Title = "Flip the Switch", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "HouseFoyer", Title = "Return to the Foyer", Type = "Maze" });
                    }
                    break;

                case "DeactivateTurret":
                    location.Description = "You flip the switch to the off position.";
                    location.AccessibleLocations.Add(new Link() { Name = "ControlRoom", Title = "Continue", Type = "Maze" });
                    break;

                case "TopWindow":
                    location.Header = "Top Room";
                    location.Description = "You climb into a top floor room through a window. There does not appear to be much in the room. There is a door leading somewehere else.";
                    location.AccessibleLocations.Add(new Link() { Name = "Hallway", Title = "Go Through the Door", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "HouseExterior", Title = "Return to the Ground", Type = "Maze" });
                    break;

                case "Hallway":
                    location.Header = "Hallway";
                    if (player.VisitedLocations.Contains("," + "DeactivateTurret" + ","))
                    {
                        location.Description = "You see an inactive turret at the end of the hallway. There is a door next to it.";
                        location.AccessibleLocations.Add(new Link() { Name = "HouseTreasury", Title = "Go Through the Door", Type = "Maze" });
                        location.AccessibleLocations.Add(new Link() { Name = "TopWindow", Title = "Go Back Into the Room", Type = "Maze" });
                    }
                    else
                    {
                        location.Description = "You walk out into the hallway. You hear a beeping noise, and see a turret at the end of the hallway. There is a door next to it.";
                        location.AccessibleLocations.Add(new Link() { Name = "Turret", Title = "Run Past the Turret", Type = "Finish" });
                        location.AccessibleLocations.Add(new Link() { Name = "TopWindow", Title = "Go Back Into the Room", Type = "Maze" });
                    }
                    break;

                case "HouseTreasury":
                    location.Header = "House Treasury";
                    location.Description = "You open the door and see a small room, in which there is a single safe. It has a combination lock on it.";
                    if (player.Inventory.Contains("," + "Combination" + ","))
                    {
                        if (player.VisitedLocations.Contains("," + "CrackedSafe" + ","))
                        {
                            location.Description = "You open the door and see a small room, in which there is a single safe. It is open.";
                        }
                        else
                        {
                            location.AccessibleLocations.Add(new Link() { Name = "CrackedSafe", Title = "Enter Your Combination into the Safe", Type = "Maze" });
                        }
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "Hallway", Title = "Go Back Into the Hallway", Type = "Maze" });
                    break;

                case "CrackedSafe":
                    location.Description = "You enter in the combination, and the safe door opens. Inside is a pile of cash and a small computer chip.";
                    location.ScoreModifier = 6;
                    location.InventoryModifier = "Electronic Chip";
                    location.AccessibleLocations.Add(new Link() { Name = "HouseTreasury", Title = "Continue", Type = "Maze" });
                    break;

                case "MazeEntrance":
                    location.Header = "Cave";
                    location.Description = "You see a small dark cave underwater. Using your flashlight, you are able to see just well enough that you could navigate through its passages.";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Enter the Cave", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "SunkenShip", Title = "Swim Back to the Ship", Type = "Maze" });
                    break;

                case "Maze1":
                    location.Header = "First Cave";
                    location.Description = "You enter a small cave with two exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze2", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze3", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "MazeEntrance", Title = "Swim Back Outside", Type = "Maze" });
                    break;

                case "Maze2":
                    location.Header = "Second Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze11", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze15", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze3":
                    location.Header = "Third Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze6", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze4", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze4":
                    location.Header = "Fourth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze5", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze3", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze3", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze5":
                    location.Header = "Fifth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze4", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze6":
                    location.Header = "Sixth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze7", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze9", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze3", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze7":
                    location.Header = "Seventh Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze8", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze10", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze6", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze8":
                    location.Header = "Eighth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze10", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze10", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze7", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze9":
                    location.Header = "Ninth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze11", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze6", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze10":
                    location.Header = "Tenth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze8", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze8", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze7", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze11":
                    location.Header = "Eleventh Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze12", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze9", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze2", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze12":
                    location.Header = "Twelfth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze13", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze14", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze11", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze13":
                    location.Header = "Thirteenth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze7", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze14", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze12", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze14":
                    location.Header = "Fourteenth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze13", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Pit", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze12", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze15":
                    location.Header = "Fifteenth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze16", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze2", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze2", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Maze16":
                    location.Header = "Sixteenth Cave";
                    location.Description = "You enter a small cave with three exits";
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the First Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze1", Title = "Go to the Second Exit", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze15", Title = "Go to the Third Exit", Type = "Maze" });
                    break;

                case "Pit":
                    location.Header = "Pit";
                    location.Description = "You enter a large pit. The sides of the pit are perfectly smooth and vertical. If you look up you can see the sky. In the center of the pit is a very large spaceship.";
                    location.AccessibleLocations.Add(new Link() { Name = "Spaceship", Title = "Enter the Ship", Type = "Maze" });
                    location.AccessibleLocations.Add(new Link() { Name = "Maze14", Title = "Return to the Cave", Type = "Maze" });
                    break;

                case "Spaceship":
                    location.Header = "Spaceship";
                    if (player.VisitedLocations.Contains("," + "RepairShip" + ","))
                    {
                        if (player.VisitedLocations.Contains("," + "AddController" + ","))
                        {
                            location.Description = "The ship is fully functional, and ready to fly.";
                            location.AccessibleLocations.Add(new Link() { Name = "SpaceFlight", Title = "Leave This Planet", Type = "Finish", RequireConfirmation = true });
                        }
                        else
                        {
                            location.Description = "The ship is in proper repair but will require a navigational computer chip to fly.";
                            if (player.Inventory.Contains("," + "Electronic Chip" + ","))
                            {
                                location.AccessibleLocations.Add(new Link() { Name = "AddController", Title = "Install the Chip", Type = "Maze" });
                            }
                        }
                    }
                    else
                    {
                        location.Description = "You enter the ship. It appears to be heavily damaged. You would need the right tools and a good deal of time to fix it.";
                        if (player.Inventory.Contains("," + "Toolkit" + ","))
                        {
                            location.AccessibleLocations.Add(new Link() { Name = "RepairShip", Title = "Use Your Toolkit to Fix the Ship", Type = "Maze" });
                        }
                    }
                    location.AccessibleLocations.Add(new Link() { Name = "Pit", Title = "Leave the Ship", Type = "Maze" });
                    break;

                case "RepairShip":
                    location.Description = "You spend a few hours fixing the spaceship. Eventually you are able to turn it on. When you do, the display screen shows the message, \"Navigational Computer Chip Missing\"";
                    location.AccessibleLocations.Add(new Link() { Name = "Spaceship", Title = "Continue", Type = "Maze" });
                    break;

                case "AddController":
                    location.Description = "You install the navigational computer chip. The ship is now ready.";
                    location.AccessibleLocations.Add(new Link() { Name = "Spaceship", Title = "Continue", Type = "Maze" });
                    break;

                default:
                    location.Description = "You are nowhere. There's probably been a mistake of some sort.";
                    location.AccessibleLocations.Add(new Link() { Name = previousLocation, Title = "Go Back", Type = "Maze" });
                    break;
            }
            
            player.CurrentLocation = location.Name;

            if (location.Name != null && player.VisitedLocations.Contains("," + location.Name+ ",") == true)
            {
                location.AlreadySeen = true;
            }

            if (location.ScoreModifier != 0 && location.AlreadySeen == false)
            {
                player.Score += location.ScoreModifier;
            }

            if (location.InventoryModifier != null && location.AlreadySeen == false)
            {
                player.Inventory += location.InventoryModifier + ",";
            }

            _dbContext.SaveChanges();

            ViewBag.Id = player.Id;
            ModelState.Clear();
            return View(location);
        }

        public ActionResult Finish(string previousLocation, string previousHeader, string inputLocation, string id)
        {
            var player = CalculateLocation(id, previousLocation, previousHeader);

            if (player == null)
            {
                return HttpNotFound();
            }

            player.Locked = true;

            FinishLocation finishLocation = new FinishLocation();
            finishLocation.Name = inputLocation;
            finishLocation.Score = player.Score;

            switch (inputLocation)
            {
                case "Exit":
                    finishLocation.Header = "You leave";
                    break;

                case "Drown":
                    finishLocation.Header = "You drown.";
                    finishLocation.Score = 0;
                    break;

                case "Turret":
                    finishLocation.Header = "The turret shoots you. You die.";
                    finishLocation.Score = 0;
                    break;

                case "Disintegrator":
                    finishLocation.Header = "You enter the shack. It is empty. There is a blinding flash of light. You die.";
                    finishLocation.Score = 0;
                    break;

                case "BeatenToDeath":
                    finishLocation.Header = "You start to walk down the road, and the person standing next to it beats you to death with the sign.";
                    finishLocation.Score = 0;
                    break;

                case "FallToDeath":
                    finishLocation.Header = "You start to walk across the gap, and then accidentally walk off the edge of the path. You hit the ground several seconds later. You die.";
                    finishLocation.Score = 0;
                    break;

                case "SpaceFlight":
                    finishLocation.Header = "You turn on the ship, and slowly lift off the ground, and into the air. You accelerate, flying out into space.";
                    finishLocation.Score += 18;
                    break;

                default:
                    finishLocation.Header = "You are nowhere. There's probably been a mistake of some sort.";
                    finishLocation.Score = 0;
                    break;
            }

            player.Score = finishLocation.Score;
            _dbContext.SaveChanges();

            return View(finishLocation);
        }

        public ActionResult Danger(string type, string inputLocation, string previousLocation, string previousHeader, string id)
        {
            var player = CalculateLocation(id, previousLocation, previousHeader);

            if (player == null)
            {
                return HttpNotFound();
            }

            player.CurrentLocation = null;
            _dbContext.SaveChanges();

            Danger danger = new Danger() { Type = type, inputLocation = inputLocation, PreviousLocation = previousLocation, Id = id };
            ModelState.Clear();
            return View(danger);
        }

        public ActionResult Menu(string previousLocation, string previousHeader, string currentLocation, string menuType, string id)
        {
            var player = CalculateLocation(id, previousLocation, previousHeader);

            if (player == null)
            {
                return HttpNotFound();
            }

            player.CurrentLocation = null;
            _dbContext.SaveChanges();

            Menu menu = new Menu();
            menu.CurrentLocation = currentLocation;
            menu.Player = player;
            menu.MenuType = menuType;

            ModelState.Clear();
            return View(menu);
        }
        
        public ActionResult Discrepancy(Player player)
        {
            return View(player);
        }

        public ActionResult Cheater()
        {
            return View();
        }
    }
}