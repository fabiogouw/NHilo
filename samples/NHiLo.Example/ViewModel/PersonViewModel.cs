using System.Collections.ObjectModel;
using System.Windows.Input;
using NHiLo.Example.Model;
using System.Collections.Specialized;
using NHiLo.Example.Repository;

namespace NHiLo.Example.ViewModel
{
    public class PersonViewModel : ViewModelBase
    {
        private readonly IPersonRepository _repository;

        private Person _selectedPerson;
        private ObservableCollection<Person> _people;
        private ObservableCollection<Contact> _contactsFromSelectedPerson;
        private Contact _selectedContact;
        private ICommand _saveCommand;
        private ICommand _addNewPersonCommand;
        private ICommand _addNewContact;
        private ICommand _deletePerson;
        private ICommand _removeContact;
        private ICommand _refreshCommand;

        #region Bindable commands & properties
        
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set
            {
                _selectedPerson = value;
                NotifyPropertyChanged("SelectedPerson");
                SelectedContact = new Contact();
                ContactsFromSelectedPerson = new ObservableCollection<Contact>(_selectedPerson.Contacts);
            }
        }

        public ObservableCollection<Person> People
        {
            get { return _people; }
            set
            {
                _people = value;
                NotifyPropertyChanged("People");
            }
        }

        public ObservableCollection<Contact> ContactsFromSelectedPerson
        {
            get { return _contactsFromSelectedPerson; }
            set
            {
                _contactsFromSelectedPerson = value;
                NotifyPropertyChanged("ContactsFromSelectedPerson");
            }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                NotifyPropertyChanged("SelectedContact");
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(p => this.Save(), null);
                return _saveCommand;
            }
        }

        public ICommand AddNewPersonCommand
        {
            get
            {
                if (_addNewPersonCommand == null)
                    _addNewPersonCommand = new RelayCommand(p => this.AddNewPerson(), null);
                return _addNewPersonCommand;
            }
        }

        public ICommand AddNewContactCommmand
        {
            get
            {
                if (_addNewContact == null)
                    _addNewContact = new RelayCommand(p => this.AddNewContact(), null);
                return _addNewContact;
            }
        }

        public ICommand DeletePersonCommand
        {
            get
            {
                if (_deletePerson == null)
                    _deletePerson = new RelayCommand(p => this.DeletePerson((Person)p), null);
                return _deletePerson;
            }
        }

        public ICommand RemoveContactCommand
        {
            get
            {
                if (_removeContact == null)
                    _removeContact = new RelayCommand(p => this.RemoveContact((Contact)p), null);
                return _removeContact;
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                    _refreshCommand = new RelayCommand(p => this.Refresh(), null);
                return _refreshCommand;
            }
        }

        #endregion

        public PersonViewModel(IPersonRepository repository)
        {
            _repository = repository;
            Refresh();
        }

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("People");
        }

        private void ContactsFromSelectedPerson_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("ContactsFromSelectedPerson");
        }

        private void Save()
        {
            _repository.SavePerson(SelectedPerson);
            if (!People.Contains(SelectedPerson))
                People.Add(SelectedPerson);
            SelectedPerson = new Person();
        }

        private void AddNewPerson()
        {
            var person = new Person();
            SelectedPerson = person;
            People.Add(person);
        }

        private void AddNewContact()
        {
            if (SelectedPerson != null)
            {
                var contact = new Contact() { Person = SelectedPerson };
                SelectedPerson.Contacts.Add(contact);
                SelectedContact = contact;
                ContactsFromSelectedPerson.Add(contact);
            }
        }

        private void DeletePerson(Person person)
        {
            _repository.DeletePerson(person);
            People.Remove(person);
            SelectedPerson = new Person();
        }

        private void RemoveContact(Contact contact)
        {
            SelectedPerson.Contacts.Remove(contact);
            ContactsFromSelectedPerson = new ObservableCollection<Contact>(SelectedPerson.Contacts);
        }

        private void Refresh()
        {
            SelectedPerson = new Person();
            SelectedContact = new Contact();
            People = new ObservableCollection<Person>(_repository.GetAllPeople());
            ContactsFromSelectedPerson = new ObservableCollection<Contact>();
            People.CollectionChanged += new NotifyCollectionChangedEventHandler(People_CollectionChanged);
            ContactsFromSelectedPerson.CollectionChanged += new NotifyCollectionChangedEventHandler(ContactsFromSelectedPerson_CollectionChanged);
        }
    }
}
