using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class LanguageQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public string Get(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.Data;
    }

    public List<Model_Language> List()
    {
        List<Model_Language> list = [.. DB.Admin_Language.Select(s => new Model_Language(s.Code, s.Name, null))];
        return list;
    }

    public Model_Language Details(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_Language result = new(item.Code, item.Name, item.Data);
        return result;
    }

    public void Update(Model_Language? model)
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
}
