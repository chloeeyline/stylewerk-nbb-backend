using Microsoft.AspNetCore.Mvc;
using Moq;
using StyleWerk.NBB.Controllers;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Tests
{
    public class EntryTest
    {
        [Fact]
        public void AddFolder()
        {
            Mock<NbbContext> folder = new();
            EntryController controller = new(folder.Object);

            Model_EntryFolders newFolder = new(null, "TestFolder", 1, new Model_EntryItem[0]);
            IActionResult result = controller.UpdateFolder(newFolder);

            Assert.NotNull(result);
        }

        [Fact]
        public void ChangeFolder()
        {

        }

        [Fact]
        public void GetEntriesInFolder()
        {

        }

        [Fact]
        public void ChangeFolderSortOrder()
        {

        }

        [Fact]
        public void AddEntryToFolder()
        {

        }

        [Fact]
        public void RemoveEntryFromFolder()
        {

        }

        [Fact]
        public void DeleteFolder()
        {

        }

        [Fact]
        public void AddEntry()
        {

        }

        [Fact]
        public void ChangeEntry()
        {

        }

        [Fact]
        public void AddTag()
        {

        }
        [Fact]
        public void RemoveTag()
        {

        }

        [Fact]
        public void ChangeTag()
        {

        }

        [Fact]
        public void GetEntriesWithoutFolder()
        {

        }

        [Fact]
        public void GetEntryPreview()
        {

        }
    }
}
