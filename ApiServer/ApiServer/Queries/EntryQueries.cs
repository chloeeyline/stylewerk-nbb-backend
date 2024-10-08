using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// Load all Entries that are available for User and filter them by the specified filters
    /// </summary>
    public List<Model_EntryItem> List(string? name, string? username, string? templateName, string? tags, bool? publicShared, bool? shared, bool? includeOwned, bool? directUser)
    {
        if (publicShared is not true && shared is not true)
            includeOwned = true;

        // Normalize the username for comparison
        username = username?.Normalize().ToLower();
        tags = tags?.Normalize().ToLower();

        // Query for shared entries (both directly and group-shared)
        var query =
        from si in DB.Share_Item
        where si.Type == ShareType.Entry
        join entry in DB.Structure_Entry on
            si.ItemID equals entry.ID
        join owner in DB.User_Login on entry.UserID
            equals owner.ID
        join template in DB.Structure_Template on
            entry.TemplateID equals template.ID
        join sgu in DB.Share_GroupUser on
            new { si.ToWhom, si.Visibility } equals
            new { ToWhom = (Guid?)sgu.GroupID, Visibility = ShareVisibility.Group }
            into groupJoin
        from sharedGroup in groupJoin.DefaultIfEmpty()
        join sg in DB.Share_Group on
            sharedGroup.GroupID equals sg.ID into groupDataJoin
        from groupData in groupDataJoin.DefaultIfEmpty()
        where (si.ToWhom == CurrentUser.ID || sharedGroup.UserID == CurrentUser.ID)
        select new
        {
            entry.ID,
            entry.Name,
            entry.IsEncrypted,
            entry.Tags,
            entry.CreatedAt,
            entry.LastUpdatedAt,
            templateName = template.Name,
            ownerUsername = owner.UsernameNormalized,
            si.Visibility
        };

        if (includeOwned is true)
        {
            // Query for owned entries
            var ownedQuery =
            from entry in DB.Structure_Entry
            join owner in DB.User_Login on
                entry.UserID equals owner.ID
            join template in DB.Structure_Template on
                entry.TemplateID equals template.ID
            where entry.UserID == CurrentUser.ID
            select new
            {
                entry.ID,
                entry.Name,
                entry.IsEncrypted,
                entry.Tags,
                entry.CreatedAt,
                entry.LastUpdatedAt,
                templateName = template.Name,
                ownerUsername = owner.UsernameNormalized,
                Visibility = ShareVisibility.None // Mark as owned
            };

            // Combine the two queries (shared + owned) using Union
            query = query.Union(ownedQuery);
        }

        // Apply visibility filters based on the model
        query = from s in query
                where
                    (includeOwned == true && s.Visibility == ShareVisibility.None) || // Include owned items if filter is true
                    (publicShared == true && s.Visibility == ShareVisibility.Public) ||
                    (shared == true && (s.Visibility == ShareVisibility.Directly || s.Visibility == ShareVisibility.Group))
                select s;

        // Apply additional filters prior to grouping
        if (!string.IsNullOrWhiteSpace(name))
            query = from s in query
                    where s.Name.Contains(name)
                    select s;

        if (!string.IsNullOrWhiteSpace(templateName))
            query = from s in query
                    where s.templateName.Contains(templateName)
                    select s;

        if (!string.IsNullOrWhiteSpace(tags))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.Tags) && tags.Contains(s.Tags)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is false)
            query = from s in query
                    where s.ownerUsername.Contains(username)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is true)
            query = from s in query
                    where s.ownerUsername == username
                    select s;

        // Group entries by ID, giving priority to owned entries over shared and public entries
        var groupedQuery =
        from s in query
        group s by s.ID into g
        select new
        {
            Entry = g.FirstOrDefault(x => x.Visibility == ShareVisibility.None) ?? // Highest priority: owned
                    g.FirstOrDefault(x => x.Visibility == ShareVisibility.Directly) ?? // Second priority: directly shared
                    g.FirstOrDefault(x => x.Visibility == ShareVisibility.Group) ??    // Third priority: group shared
                    g.FirstOrDefault(x => x.Visibility == ShareVisibility.Public)      // Lowest priority: public
        };

        // Apply ordering before final selection
        IQueryable<Model_EntryItem> orderedQuery =
        from g in query
        orderby g.Visibility, g.LastUpdatedAt, g.Name
        select new Model_EntryItem
        (
            g.ID,
            g.Name,
            g.IsEncrypted,
            g.Tags,
            g.CreatedAt,
            g.LastUpdatedAt,
            g.templateName,
            g.ownerUsername,
            g.Visibility
        );

        // Execute the query and return the results
        return [.. orderedQuery];
    }

    /// <summary>
    /// Removes an entry and all its Details
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
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
