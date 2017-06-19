using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGenerator.Templates;

namespace wServer.realm.worlds
{
    public static class DungeonTemplates
    {
        private static readonly List<Type> Templates;

        static DungeonTemplates()
        {
            Templates = new List<Type>();

            var type = typeof (DungeonTemplate);
            var templates = type.Assembly.GetTypes()
                .Where(t => type.IsAssignableFrom(t) && type != t);

            foreach (var i in templates)
                Templates.Add(i);
        }

        public static DungeonTemplate GetTemplate(string worldName)
        {
            var template = $"{worldName}Template";
            foreach (var type in Templates)
            {
                if (!type.Name.Equals(template))
                    continue;

                return (DungeonTemplate)Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
