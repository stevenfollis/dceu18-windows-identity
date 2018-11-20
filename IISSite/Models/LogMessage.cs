using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISSite.Models
{
    [Serializable]
    public class LogMessage
    {
        public string Message { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }

        public LogMessage()
        {
            Created = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
        }
        public override string ToString()
        {
            return string.Format("Sample Message Id:[{0}] Name:[{1}] Created:[{2}]", Id, Name, Created);
        }
    }
}