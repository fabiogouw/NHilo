using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHiLo.Example.Model;
using NHiLo.Common;
using Microsoft.EntityFrameworkCore;

namespace NHiLo.Example.Repository
{
    public class SQLitePersonRepository : IPersonRepository
    {
        private readonly IKeyGenerator<long> _personKeyGenerator;
        private readonly IKeyGenerator<string> _contactKeyGenerator;

        //public SQLitePersonRepository(IKeyGenerator<long> personKeyGenerator, IKeyGenerator<string> contactKeyGenerator)
        //{
        //    _personKeyGenerator = personKeyGenerator;
        //    _contactKeyGenerator = contactKeyGenerator;
        //}

        public List<Person> GetAllPeople()
        {
            using (var context = new PersonDbContext())
            {
                var people = (from p in context.People.Include(p => p.Contacts) select p).ToList();
                return people;
            }
        }

        public void SavePerson(Person person)
        {
            person = SetIdsForNewEntities(person);
            using (var context = new PersonDbContext())
            {
                var personInDb = context.People.Include(p => p.Contacts).SingleOrDefault(p => p.Id == person.Id);
                if (personInDb == null)
                    InsertPerson(context, person);
                else
                    UpdatePerson(context, person, personInDb);
                context.SaveChanges();
            }
        }

        private void InsertPerson(PersonDbContext context, Person person)
        {
            context.People.Add(person);
        }

        private void UpdatePerson(PersonDbContext context, Person personWithNewData, Person personInDb)
        {
            context.Entry<Person>(personInDb).CurrentValues.SetValues(personWithNewData);
            var contactsInDb = personInDb.Contacts.ToList();
            foreach (var contactInDb in contactsInDb)
            {
                var contact = personWithNewData.Contacts.SingleOrDefault(i => i.Id == contactInDb.Id);
                if (contact != null)
                    context.Entry(contactInDb).CurrentValues.SetValues(contact);
                else
                    context.Contacts.Remove(contactInDb);
            }
            foreach (var contact in personWithNewData.Contacts)
            {
                if (!personInDb.Contacts.Any(c => c.Id == contact.Id))
                {
                    contact.Person = personInDb;
                    personInDb.Contacts.Add(contact);
                }
            }
        }

        private Person SetIdsForNewEntities(Person person)
        {
            // all this example code to justify this part..., using NHiLo to generate new ids
            if (person.Id <= 0)
                person.Id = _personKeyGenerator.GetKey();
            foreach (var contact in person.Contacts)
            {
                // here the id is a guid, so we call the guid generator
                if (string.IsNullOrEmpty(contact.Id))
                    contact.Id = _contactKeyGenerator.GetKey();
            }
            return person;
        }

        public void DeletePerson(Person person)
        {
            using (var context = new PersonDbContext())
            {
                var personInDb = context.People.SingleOrDefault(p => p.Id == person.Id);
                if (personInDb != null)
                {
                    context.People.Remove(personInDb);
                    context.SaveChanges();
                }
            }
        }
    }
}
