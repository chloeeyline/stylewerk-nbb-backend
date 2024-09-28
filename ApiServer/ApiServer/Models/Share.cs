using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_ShareItem(Guid ID, Guid ItemID, string WhoShared, ShareVisibility Visibility, bool CanShare, bool CanEdit, bool CanDelete, string? ToWhom)
{
    public Model_ShareItem(Share_Item item, string? toWhom) : this(item.ID, item.ItemID, item.O_User.Username, item.Visibility, item.CanShare, item.CanEdit, item.CanDelete, toWhom) { }
}
public record Model_Share(Guid ID, string? ToWhom, ShareVisibility Visibility, ShareType Type, bool CanShare, bool CanEdit, bool CanDelete);

public record Model_Group(Guid ID, string Name, int UserCount);
public record Model_GroupUser(string Username, Guid GroupID, bool CanAddUsers, bool CanRemoveUsers, string? WhoAdded);
public record Model_SharedToGroup(Guid ID, ShareType Type, string Name);
