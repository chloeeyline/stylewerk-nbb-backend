﻿using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models;

///OverView List Request
public record Model_EntryFolders(Guid? ID, string? Name, int SortOrder, Model_EntryItem[] Items);

public record Model_EntryItem(Guid ID, string Name, string? FolderName, string UserName, string TemplateName, string? Tags, long CreatedAt, long LastUpdatedAt, ShareTypes Share)
{
    public Model_EntryItem(Structure_Entry item, ShareTypes share) : this(item.ID, item.Name, item.O_Folder?.Name, item.O_User.Username, item.O_Template.Name, item.Tags, item.CreatedAt, item.LastUpdatedAt, share) { }
}

/// <summary>
/// When a Entry is moved into a new Folder
/// </summary>
/// <param name="EntryID">The Entry that is move</param>
/// <param name="FolderID">
/// The Folder in which it should be placed
/// <br/>
/// If null that means it should be placed out of Folders in general
/// </param>
public record Model_ChangeFolder(Guid EntryID, Guid? FolderID);
public record Model_ChangeEntryName(Guid EntryID, string Name);


/// <summary>
/// The filters send from the Frontend to get the resulting Folders and Entrys that match the Filter
/// </summary>
/// <param name="Name"></param>
/// <param name="Username"></param>
/// <param name="TemplateName"></param>
/// <param name="Tags"></param>
/// <param name="Share"></param>
/// <param name="DirectUser"></param>
public record Model_FilterEntry(string? Name, string? Username, string? TemplateName, string? Tags, ShareTypes Share, bool DirectUser);
public record Model_AddEntry(string Name, Guid TemplateID, Guid? FolderID);
public record Model_FolderSortOrder(Guid ID, int SortOrder);
public record Model_ListFolderSortOrder(List<Model_FolderSortOrder> FolderSortOrders);
