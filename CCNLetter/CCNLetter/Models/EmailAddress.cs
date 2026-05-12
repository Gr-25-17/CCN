using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CCNLetter.Models
{
    internal class EmailAddress
    {
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Address { get; set; } = string.Empty;
    }
}
