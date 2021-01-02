using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus
{
    public class ContextSentMessage
    {
        public Type Type { get; set; }
        public object Message { get; set; }
        public int QueueId { get; set; }
        public DateTime? NotBefore { get; set; }
    }
}
