using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace common.resources
{
    public class PrivateMessages
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(PrivateMessages));

        [JsonProperty("ownerAccountId")]
        public int OwnerAccountId { get; private set; }
        [JsonProperty("messages")]
        public List<PrivateMessage> Messages { get; private set; }

        public PrivateMessages(int ownerId, List<PrivateMessage> messages)
        {
            OwnerAccountId = ownerId;
            Messages = messages;
        }

        public void PrepareForSend(Database db)
        {
            foreach (var message in Messages)
                message.PrepareForSend(db);

            Messages = Messages.OrderByDescending(_ => _.ReceiveTime).ToList();
        }

        public Task DelteMessage(Database db, int time)
        {
            return Task.Factory.StartNew(() =>
            {
                var message = Messages.FirstOrDefault(_ => _.ReceiveTime == time);
                var acc = db.GetAccount(OwnerAccountId);
                if (message == null || acc == null || acc.IsNull)
                    return;

                Messages.Remove(message);

                acc.PrivateMessages = this;
                acc.FlushAsync().Wait();
            }).ContinueWith(e =>
                Log.Error(e.Exception.InnerException.ToString()),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        [JsonObject(Id="privateMessage")]
        public class PrivateMessage
        {
            [JsonProperty("senderId")]
            public int SenderId { get; }
            [JsonProperty("recipientId")]
            public int RecipientId { get; }
            [JsonProperty("subject")]
            public string Subject { get; }
            [JsonProperty("message")]
            public string Message { get; }
            [JsonProperty("receiveTime")]
            public int ReceiveTime { get; }

            [JsonProperty("senderName")]
            public string SenderName { get; private set; }
            [JsonProperty("recipientName")]
            public string RecipientName { get; private set; }

            public PrivateMessage(int senderId, int recipientId, string subject, string message, int receiveTime)
            {
                SenderId = senderId;
                RecipientId = recipientId;
                Subject = subject;
                Message = message;
                ReceiveTime = receiveTime;
            }

            public void PrepareForSend(Database db)
            {
                SenderName = db.ResolveIgn(SenderId);
                RecipientName = db.ResolveIgn(RecipientId);
            }
        }

        public string ToJson()
        {
            var serializer = new JsonSerializer();
            var wtr = new StringWriter();
            serializer.Serialize(wtr, this);
            return wtr.ToString();
        }

        public bool NeedsFix()
        {
            // To make changes easier
            return OwnerAccountId == 0;
        }

        public void FixFromOldBuild(DbAccount acc)
        {
            OwnerAccountId = acc.AccountId;
        }
    }
}
