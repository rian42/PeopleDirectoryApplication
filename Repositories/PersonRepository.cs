using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Models;
using PeopleDirectoryApplication.Repositories.Interfaces;

namespace PeopleDirectoryApplication.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Person>> GetAllPersons() => await _context.Persons.AsNoTracking().ToListAsync();

        public Person GetPersonById(int PersonId)
            => _context.Persons.Where(u => u.Id == PersonId).AsNoTracking().FirstOrDefault();

        public async Task<IEnumerable<Person>> GetPersonsBySearchTerm(string searchTerm)
            => await _context.Persons.Where(u => u.Name.Contains(searchTerm)).AsNoTracking().ToListAsync();

        public void InsertPerson(Person Person)
        {
            if (Person == null)
                throw new ArgumentNullException(nameof(Person));

            _context.Persons.Add(Person);
            _context.SaveChanges();
        }

        public void UpdatePerson(Person Person)
        {
            if (Person == null)
                throw new ArgumentNullException(nameof(Person));

            var entry = _context.Persons.Find(Person.Id);
            var PersonProperties = typeof(Person).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in PersonProperties)
            {
                var newValue = prop.GetValue(Person);

                if (newValue != null)
                    prop.SetValue(entry, newValue);
            }

            _context.Persons.Update(entry!);

            _context.SaveChanges();
        }

        public void DeletePerson(int PersonId)
        {
            var Person = _context.Persons.Find(PersonId);

            if (Person != null)
            {
                _context.Persons.Remove(Person);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Person> GetMatchingRecords(string query)
            => _context.Persons.Where(p => p.Name.Contains(query));
    }
}
