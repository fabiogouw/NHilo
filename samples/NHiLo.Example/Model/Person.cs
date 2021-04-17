using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHiLo.Example.Model
{
    public class Person
    {
        public Person()
        {
            Contacts = new List<Contact>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}
