namespace uHub.Databse.Entities
{
    public class Entity
    {
        public int id;
        public string name;
        public string description;
        public int level;

        public Entity(string name, string description = default(string), int level = 1)
        {
            this.name = name;
            this.description = description;
            this.level = level;
        }
    }
}