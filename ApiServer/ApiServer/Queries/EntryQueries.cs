using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_EntryItem> List(string? name, string? username, string? templateName, string? tags, bool? includePublic)
    {
        username = username?.Normalize().ToLower();
        tags = tags?.Normalize().ToLower();

        IQueryable<Structure_Entry> query = DB.Structure_Entry.Include(s => s.O_User).Include(s => s.O_Template).Where(s => s.O_User.UsernameNormalized == username || (s.IsPublic && includePublic == true));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(templateName))
            query = query.Where(s => s.O_Template.Name.Contains(templateName));

        if (!string.IsNullOrWhiteSpace(tags))
            query = query.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && s.Tags.Contains(tags));

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(s => s.O_User.UsernameNormalized.Contains(username));

        List<Model_EntryItem> result = [.. query.OrderBy(s => s.LastUpdatedAt)
            .Select(s => new Model_EntryItem(s.ID, s.FolderID ?? Guid.Empty, s.Name, s.IsEncrypted, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username))];

        return result;
    }

    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry entry = DB.Structure_Entry.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        if (entry.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);
        DB.Structure_Entry.Remove(entry);

        DB.SaveChanges();
    }
}
