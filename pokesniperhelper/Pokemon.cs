using System;

namespace pokesniperhelper
{
    internal class Pokemon
    {
        
        public string id
        {
            get; set;
        }
        public string name
        {
            get; set;
        }

        public string coords
        {
            get; set;
        }

        private string icon;
        private string until;

        public Pokemon(string id, string name, string coords, string until, string icon)
        {
            this.id = id;
            if (name.ToLower() == "mr. mime")
                this.name = "MrMime";
            this.name = name;
            this.coords = coords;
            this.until = until;
            this.icon = icon;
        }

        public override string ToString()
        {
            return String.Format("({0}){1}={2}", id, name, coords);
        }
    }
}