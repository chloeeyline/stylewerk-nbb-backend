using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Share"), Authorize]
    public class ShareController(NbbContext db) : BaseController(db)
    {
        public TemplateQueries Query => new(DB, CurrentUser);

        [HttpPost(nameof(ShareTemplate))]
        public IActionResult ShareTemplate([FromBody] Model_ShareTemplate shareTemplate)
        {
            if (shareTemplate is null)
                throw new RequestException(ResultType.DataIsInvalid);

            //check if Template Exists
            Structure_Template template = DB.Structure_Template.FirstOrDefault(t => t.ID == shareTemplate.TemplateId)
                ?? throw new RequestException(ResultType.NoDataFound);

            if (shareTemplate.Share.GroupShared)
            {
                //check if Group exists
                Share_Group group = DB.Share_Group.FirstOrDefault(g => g.ID == shareTemplate.ShareId)
                    ?? throw new RequestException(ResultType.NoDataFound);

                //check if User is in Group
                bool isInGroup = DB.Share_GroupUser.Any(g => g.GroupID == group.ID && g.UserID == CurrentUser.ID);
                if (!isInGroup)
                    throw new RequestException(ResultType.MissingRight);

                Share(shareTemplate, group.ID, shareTemplate.Share.GroupShared);
            }

            if (shareTemplate.Share.DirectlyShared)
            {
                //check if User exists
                Database.User.User_Information userExists = DB.User_Information.FirstOrDefault(u => u.ID == shareTemplate.ShareId)
                    ?? throw new RequestException(ResultType.GeneralError);

                Share(shareTemplate, userExists.ID, shareTemplate.Share.GroupShared);
            }

            if (shareTemplate.Share.Public)
            {
                template.IsPublic = true;
                DB.SaveChanges();
            }

            return Ok(new Model_Result<string>());
        }

        private void Share(Model_ShareTemplate shareTemplate, Guid toWhom, bool type)
        {
            Share_Item newShareItem = new()
            {
                ID = Guid.NewGuid(),
                WhoShared = CurrentUser.ID,
                Group = type,
                ItemType = 2,
                ItemID = shareTemplate.TemplateId,
                ToWhom = toWhom,
                CanShare = shareTemplate.Rights.CanShare,
                CanDelete = shareTemplate.Rights.CanDelete,
                CanEdit = shareTemplate.Rights.CanEdit
            };

            DB.Share_Item.Add(newShareItem);
            DB.SaveChanges();
        }

    }
}
