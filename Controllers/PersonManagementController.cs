using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleDirectoryApplication.Models;
using PeopleDirectoryApplication.Repositories.Interfaces;

namespace PeopleDirectoryApplication.Controllers
{
    public class PersonManagementController : Controller
    {
        private readonly IPersonRepository _personRepo;

        public PersonManagementController(IPersonRepository PersonRepository)
        {
            _personRepo = PersonRepository;
        }

        [Authorize]
        public async Task<IActionResult> Persons()
        {
            var Persons = await GetAllPersons();

            if (Persons == null)
                return NotFound();

            return View(Persons);
        }

        [Authorize]
        public IActionResult Profile(int Id)
        {
            var Person = _personRepo.GetPersonById(Id);

            if (Person == null)
                return NotFound();

            return View(Person);
        }

        public void UpdatePerson(Person PersonToUpdate)
            => _personRepo.UpdatePerson(PersonToUpdate);

        public void AddPerson(Person PersonToAdd)
            => _personRepo.InsertPerson(PersonToAdd);

        public void DeletePerson(Person PersonToDelete)
            => _personRepo.DeletePerson(PersonToDelete.Id);

        public async Task<IEnumerable<Person>> GetAllPersons()
        {
            var Persons = await _personRepo.GetAllPersons();

            return Persons ?? Enumerable.Empty<Person>();
        }

        public ActionResult<IEnumerable<Person>> Autocomplete(string query)
        {
            var results = _personRepo.GetMatchingRecords(query);

            return new JsonResult(results);
        }
    }
}
