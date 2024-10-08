using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_ShareItem(Guid ID, Guid ItemID, string WhoShared, ShareVisibility Visibility, ShareType Type, string? ToWhom);

public record Model_Group(Guid ID, string Name, int UserCount);
public record Model_GroupUser(string Username, Guid GroupID, bool CanAddUsers, bool CanRemoveUsers, string? WhoAdded);
public record Model_SharedToGroup(Guid ID, ShareType Type, string Name);
