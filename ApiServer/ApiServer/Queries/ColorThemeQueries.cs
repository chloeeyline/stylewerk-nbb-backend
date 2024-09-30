using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ColorThemeQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_ColorTheme> List()
    {
        List<Model_ColorTheme> list = [.. DB.Admin_ColorTheme.Select(s => new Model_ColorTheme(s.ID, s.Name, s.Base, null))];
        return list;
    }

    public Model_ColorTheme Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ColorTheme result = new(item.ID, item.Name, item.Base, item.Data);
        return result;
    }

    public string Get(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.Data;
    }

    public void Update(Model_ColorTheme? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Data))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme? item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            item = new Admin_ColorTheme()
            {
                ID = Guid.Empty,
                Name = model.Name,
                Data = model.Data,
                Base = model.Base
            };
            DB.Admin_ColorTheme.Add(item);
        }
        else
        {
            item.Name = model.Name;
            item.Base = model.Base;
            item.Data = model.Data;
        }
        DB.SaveChanges();
    }
}
