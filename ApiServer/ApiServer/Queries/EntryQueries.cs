using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_EntryItem> List(string? name, string? username, string? templateName, string? tags, bool? includePublic)
    {
        username = username?.Normalize().ToLower().Trim();
        tags = tags?.Normalize().ToLower().Trim();
        name = name?.Normalize().ToLower().Trim();
        templateName = templateName?.Normalize().ToLower().Trim();

        IQueryable<Structure_Entry> query = DB.Structure_Entry.Include(s => s.O_User).Include(s => s.O_Template);

        query = string.IsNullOrWhiteSpace(username)
            ? query.Where(s => s.UserID == CurrentUser.ID || (s.IsPublic && includePublic == true))
            : query.Where(s => s.O_User.UsernameNormalized.Contains(username));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.NameNormalized.Contains(name));

        if (!string.IsNullOrWhiteSpace(templateName))
            query = query.Where(s => s.O_Template.NameNormalized.Contains(templateName));

        if (!string.IsNullOrWhiteSpace(tags))
        {
            tags = string.Join(",", tags.Split(',').Order());
            query = query.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && s.Tags.Contains(tags));
        }

        List<Model_EntryItem> result = [.. query.OrderBy(s => s.O_User.UsernameNormalized).ThenBy(s => s.LastUpdatedAt)
            .Select(s => new Model_EntryItem(s.ID, s.Name, s.IsEncrypted, s.IsPublic, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username, s.UserID == CurrentUser.ID))];

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
