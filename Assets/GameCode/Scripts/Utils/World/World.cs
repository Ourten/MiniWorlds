using System.Collections.Generic;

namespace GameCode.Scripts.Utils.World
{
    public class World
    {
        public static World Instance = new World();

        private readonly Dictionary<string,Planet> _planets;

        private World()
        {
            _planets = new Dictionary<string, Planet>();
        }

        public void AddPlanet(string name, int length, PlanetConfig config)
        {
            _planets.Add(name, new Planet(name, length, config));
        }

        public Planet GetPlanet(string name)
        {
            return _planets[name];
        }
    }

}