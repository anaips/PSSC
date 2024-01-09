using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharp.Choices;

namespace ProiectPSSC.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class ComandaPublishedEvent
    {
        public interface IComandaPublishedEvent { }

        public record ComandaPublishScucceededEvent : IComandaPublishedEvent
        {
            public string Csv { get; }
            public DateTime PublishedDate { get; }

            internal ComandaPublishScucceededEvent(string csv, DateTime publishedDate)
            {
                Csv = csv;
                PublishedDate = publishedDate;
            }
        }

        public record ComandaPublishFaildEvent : IComandaPublishedEvent
        {
            public string Reason { get; }

            internal ComandaPublishFaildEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
