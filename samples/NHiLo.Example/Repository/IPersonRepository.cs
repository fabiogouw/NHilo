using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHiLo.Example.Model;

namespace NHiLo.Example.Repository
{
    public interface IPersonRepository
    {
        List<Person> GetAllPeople();
        void SavePerson(Person person);
        void DeletePerson(Person person);
    }
}
