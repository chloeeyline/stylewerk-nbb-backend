using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record GroupUserRights(bool CanSeeUsers, bool CanAddUsers, bool CanRemoveUsers);
public record ShareRights(bool CanShare, bool CanEdit, bool CanDelete);
public record Model_ShareDirectly(string Username, int ItemType, Guid ShareItem, ShareTypes Share, ShareRights Rights);
public record Model_ShareGroup(Guid GroupId, int ItemType, Guid ShareItem, ShareTypes Share, ShareRights Rights);
public record Model_CreateGroup(string Name, bool IsVisible, bool CanSeeOthers);
public record Model_GroupUser(string Username, Guid GroupId, GroupUserRights UserRights);
public record Model_RemoveUser(string Username, Guid GroupId);
public record Model_GroupRight(Guid GroupId, bool IsVisible, bool CanSeeOthers);
public record Model_GroupName(Guid GroupId, string GroupName);
public record Model_Group(Guid GroupId, string Name, bool IsVisible, bool CanSeeOthers, Model_GroupUser[] Users);
public record Model_User(Guid UserId, string Username);
public record Model_UserDetails(string Username, Guid ShareItem);
public record Model_GroupDetails(Guid GroupId, Guid ShareItem);
public record Model_ShareItemRightsUser(Guid ShareItem, string Username, ShareRights Rights);
public record Model_ShareItemRightsGroup(Guid ShareItem, Guid? GroupId, ShareRights Rights);

#region NewModels
public record Model_ShareItem(Guid ID, string WhoShared, string ToWhom, ShareVisibility Visibility, ShareRight Rights);
public record Model_Share(Guid ID, string? ToWhom, ShareVisibility Visibility, byte Type, ShareRight Rights);
public record Model_UserFromGroup2(string UserName, Guid GroupID, GroupRight2 Rights);

public record Model_Group2(Guid ID, string Name, bool IsVisible, bool CanSeeOthers, int UserCount);
public record Model_GroupUser2(string Username, string WhoAdded, GroupRight2 Rights);

public record GroupRight2(bool CanSeeUsers, bool CanAddUsers, bool CanRemoveUsers);
public record ShareRight(bool CanShare, bool CanEdit, bool CanDelete);
#endregion
