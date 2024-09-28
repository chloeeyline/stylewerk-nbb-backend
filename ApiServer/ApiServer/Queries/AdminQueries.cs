using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class AdminQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    #region Language
    public List<Model_Language> GetLanguageList()
    {
        List<Model_Language> list = [.. DB.Admin_Language.Select(s => new Model_Language(s.Code, s.Name, null))];
        return list;
    }

    public string GetLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.Data;
    }

    public Model_Language GetLanguageDetails(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_Language result = new(item.Code, item.Name, item.Data);
        return result;
    }

    public void SaveLanguage(Model_Language? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Data))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language? item = DB.Admin_Language.FirstOrDefault(s => s.Code == model.Code);

        if (item is null)
        {
            item = new Admin_Language()
            {
                Code = model.Code,
                Name = model.Name,
                Data = model.Data,
            };
            DB.Admin_Language.Add(item);
        }
        else
        {
            item.Name = model.Name;
            item.Data = model.Data;
        }
        DB.SaveChanges();
    }
    #endregion

    #region Color Theme
    public List<Model_ColorTheme> GetThemeList()
    {
        List<Model_ColorTheme> list = [.. DB.Admin_ColorTheme.Select(s => new Model_ColorTheme(s.ID, s.Name, s.Base, null))];
        return list;
    }

    public string GetTheme(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.Data;
    }

    public Model_ColorTheme GetThemeDetails(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ColorTheme result = new(item.ID, item.Name, item.Base, item.Data);
        return result;
    }

    public void SaveTheme(Model_ColorTheme? model)
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
    #endregion
}
