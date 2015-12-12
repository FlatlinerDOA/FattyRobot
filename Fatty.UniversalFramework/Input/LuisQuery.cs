using System.Collections.ObjectModel;

namespace Fatty.UniversalFramework.Input
{
    public sealed class LuisQuery
    {
        public LuisQuery()
        {
            this.Intents = new Collection<LuisIntent>();
        }

        public string Query
        {
            get;
            set;
        }

        public Collection<LuisIntent> Intents
        {
            get;
            set;
        }

        public Collection<LuisEntity> Entities
        {
            get;
            set;
        }
    }
}