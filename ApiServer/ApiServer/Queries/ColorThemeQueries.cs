using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ColorThemeQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public string Get(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.Data;
    }

    public List<Model_ColorTheme> List()
    {
        List<Model_ColorTheme> list = [.. DB.Admin_ColorTheme.Select(s => new Model_ColorTheme(s.ID, s.Name, s.Base, null))];
        return list;
    }

    public Model_ColorTheme Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ColorTheme result = new(item.ID, item.Name, item.Base, item.Data);
        return result;
    }

    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        DB.Admin_ColorTheme.Remove(item);
        DB.SaveChanges();
    }

    public void Update(Model_ColorTheme? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Data))
            throw new RequestException(ResultCodes.DataIsInvalid);
        if (!CurrentUser.Login.Admin)
            throw new RequestException(ResultCodes.UserMustBeAdmin);
        string name = model.Name.NormalizeName();
        Admin_ColorTheme? item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            if (DB.Admin_ColorTheme.Any(s => s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);
            item = new Admin_ColorTheme()
            {
                ID = Guid.Empty,
                Name = model.Name,
                NameNormalized = model.Name.NormalizeName(),
                Data = model.Data,
                Base = model.Base
            };
            DB.Admin_ColorTheme.Add(item);
        }
        else
        {
            if (item.NameNormalized != name && DB.Admin_ColorTheme.Any(s => s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);
            item.Name = model.Name;
            item.Base = model.Base;
            item.Data = model.Data;
        }
        DB.SaveChanges();
    }
}
