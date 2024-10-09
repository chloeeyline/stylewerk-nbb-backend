namespace ApiServerTest.Tests
{
    public class ShareTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private string Username = "TestUser";
        private string Email = "chloe.hauer@lbs4.salzburg.at";
        private string Password = "TestTest@123";

        private string OtherUsername = "TestUser1";
        private string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Remove
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();

        }

        [Fact]
        public void RemoveUserUser()
        {

        }
        #endregion

        [Fact]
        public void ChangeUserRightsInGroup()
        {

        }

        [Fact]
        public void DeleteGroup()
        {

        }

        [Fact]
        public void ShareEntryGroup()
        {

        }

        [Fact]
        public void ShareTemplateGroup()
        {

        }

        [Fact]
        public void ShareEntryDirectly()
        {

        }

        [Fact]
        public void SharetemplateDirectly()
        {

        }

        [Fact]
        public void ShareTemplatePublic()
        {

        }

        [Fact]
        public void ShareEntryPublic()
        {

        }

        [Fact]
        public void RemoveTemplatePublic()
        {

        }

        [Fact]
        public void RemoveEntryPublic()
        {

        }

        [Fact]
        public void RemoveEntryGroup()
        {

        }

        [Fact]
        public void RemoveTemplateGroup()
        {

        }

        [Fact]
        public void RemoveEntryDirect()
        {

        }

        [Fact]
        public void RemoveTemplateDirect()
        {

        }

        [Fact]
        public void ChangeRightsTemplate()
        {

        }

        [Fact]
        public void ChangeRightsEntry()
        {

        }
    }
}
