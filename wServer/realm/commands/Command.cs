using System;
using System.Collections.Generic;
using wServer.realm.entities;
using log4net;

namespace wServer.realm.commands
{
    public abstract class Command
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(Command));

        public string CommandName { get; private set; }
        public string Alias { get; private set; }
        public int PermissionLevel { get; private set; }
        public bool ListCommand { get; private set; }

        protected Command(string name, int permLevel = 0, string alias = null, bool listCommand = true)
        {
            CommandName = name;
            PermissionLevel = permLevel;
            ListCommand = listCommand;
            Alias = alias;
        }
        
        protected abstract bool Process(Player player, RealmTime time, string args);

        static int GetPermissionLevel(Player player)
        {
            return player.Client.Account.Rank;
        }

        public bool HasPermission(Player player)
        {
            return GetPermissionLevel(player) >= PermissionLevel;
        }

        public bool Execute(Player player, RealmTime time, string args, bool bypassPermission = false)
        {
            if (!bypassPermission && !HasPermission(player))
            {
                player.SendError("No permission!");
                return false;
            }

            try
            {
                return Process(player, time, args);
            }
            catch (Exception ex)
            {
                Log.Error("Error when executing the command.", ex);
                player.SendError("Error when executing the command.");
                return false;
            }
        }
    }

    public class CommandManager
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(CommandManager));

        private RealmManager _manager;
        private readonly Dictionary<string, Command> _cmds;
        public IDictionary<string, Command> Commands { get { return _cmds; } }

        public CommandManager(RealmManager manager)
        {
            _manager = manager;
            _cmds = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
            var t = typeof(Command);
            foreach (var i in t.Assembly.GetTypes())
                if (t.IsAssignableFrom(i) && i != t)
                {
                    var instance = (i.GetConstructor(new Type[] { typeof(RealmManager) }) == null) ?
                        (Command)Activator.CreateInstance(i) :
                        (Command)Activator.CreateInstance(i, manager);
                    _cmds.Add(instance.CommandName, instance);
                    if (instance.Alias != null)
                    {
                        _cmds.Add(instance.Alias, instance);
                    }
                }
        }

        public bool Execute(Player player, RealmTime time, string text)
        {
            var index = text.IndexOf(' ');
            var cmd = text.Substring(1, index == -1 ? text.Length - 1 : index - 1);
            var args = index == -1 ? "" : text.Substring(index + 1);

            Command command;
            if (!_cmds.TryGetValue(cmd, out command))
            {
                player.SendError("Unknown command!");
                return false;
            }

            var owner = player.Owner;
            Log.InfoFormat("[Command] [{0}] <{1}> {2}", owner?.Name ?? "", player.Name, text);
            return command.Execute(player, time, args);
        }
    }
}
