using System.ComponentModel;

namespace common
{
    public enum Channel
    {
        [Description("Network")]
        Network,
        [Description("Control")]
        Control,
        [Description("Chat")]
        Chat
    }

    public enum ChatType
    {
        Tell,
        Guild,
        Announce,
        GuildAnnounce,
        Invite,
        Info
    }

    public enum NetworkCode
    {
        Join,
        Ping,
        Quit,
        Timeout
    }

    public enum ControlType
    {
        Reboot,
        PrivateMessageRefresh
    }

    public struct NetworkMsg
    {
        public NetworkCode Code;
        public ServerInfo Info;
    }

    public struct ChatMsg
    {
        public ChatType Type;
        public string Inst;
        public int ObjId;
        public int Stars;
        public int Admin;
        public bool Hidden;
        public int From;
        public int To;
        public string Text;
        public string SrcIP;
    }

    public struct ControlMsg
    {
        public ControlType Type;
        public string TargetInst;
        public string Issuer;
        public string Payload;
        public int Delay;
    }
}
