using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_ShareItem(Guid ID, Guid ItemID, string WhoShared, string ToWhom, ShareVisibility Visibility, ShareRight Rights);
public record Model_Share(Guid ID, string? ToWhom, ShareVisibility Visibility, ShareType Type, ShareRight Rights);

public record Model_Group2(Guid ID, string Name, int UserCount);
public record Model_GroupUser2(string Username, Guid GroupID, bool CanAddUsers, bool CanRemoveUsers, string? WhoAdded);
public record Model_SharedToGroup(Guid ID, ShareType Type, string Name);

public record ShareRight(bool CanShare, bool CanEdit, bool CanDelete);
