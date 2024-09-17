using Microsoft.AspNetCore.Mvc;
using StyleWerk.NBB.Queries;
using StyleWerk.NBB.Queries.Dto;
using System.Text.Json;

namespace StyleWerk.NBB.Controllers
{
    public class EntryOverviewController : Controller
    {
        private readonly EntryQueries _entryQueries;

        public EntryOverviewController(EntryQueries entryQueries)
        {
            _entryQueries = entryQueries;
        }

        List<EntryViewModel> Entries { get; set; }
        EntryContentViewModel Entry { get; set; }


        [HttpGet]
        public IActionResult GetEntriesByUserId(Guid userId)
        {
            Entries = _entryQueries.LoadEntriesByUserId(userId);
            return Ok(Entries);
        }

        [HttpGet]
        public IActionResult GetEntryContent(Guid userId)
        {
            Entry = _entryQueries.LoadEntryByUserId(userId);
            return Ok(Entry);
        }

        [HttpPost]
        public IActionResult SaveEntry()
        {

        }

    }
}
