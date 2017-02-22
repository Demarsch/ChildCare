
namespace UserMessageModule
{
    public class OpenUserMessageEventData
    {
        public OpenUserMessageEventData(int umId, int umTypeId, string umText, string umTag)
        {
            UserMessageId = umId;
            UserMessageTypeId = umTypeId;
            MessageText = umText;
            MessageTag = umTag;
        }

        public int UserMessageId { get; set; }
        public int UserMessageTypeId { get; set; }
        public string MessageText { get; set; }
        public string MessageTag { get; set; }
    }
}
