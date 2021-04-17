using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHiLo.Example.Model
{
    public class Contact
    {
        public string Id { get; set; }
        public Person Person { get; set; }
        public string TelephoneNumber { get; set; }
    }
}
