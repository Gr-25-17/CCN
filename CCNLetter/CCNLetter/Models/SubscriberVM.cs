using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Models
{
    public class SubscriberVM
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool ReceivesNewsletter { get; set; }
        public string PreferredCategories { get; set; } = string.Empty;
    }
}
