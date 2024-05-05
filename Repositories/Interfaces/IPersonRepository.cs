using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Repositories.Interfaces
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllPersons();
        Task<IEnumerable<Person>> GetPersonsBySearchTerm(string searchTerm);
        Person GetPersonById(int PersonId);
        void UpdatePerson(Person Person);
        void InsertPerson(Person Person);
        void DeletePerson(int PersonId);
        IEnumerable<Person> GetMatchingRecords(string query);
    }
}
