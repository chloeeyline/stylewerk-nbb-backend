using StyleWerk.NBB.Authentication;

namespace StyleWerk.NBB.Models;

public record GroupUserRights(bool CanSeeUsers, bool CanAddUsers, bool CanRemoveUsers);
public record ShareTypes(bool Own, bool GroupShared, bool Public, bool DirectlyShared);
public record ShareRights(bool CanShare, bool CanEdit, bool CanDelete);
public record Model_SharedItem(Guid ID, string SharedFrom, bool FromGroup, Guid? GroupID, string? GroupName, bool CanShare, bool CanEdit, bool CanDelete);

/// <summary>
/// The default Model to always wrap the results of a requests from the frontend
/// </summary>
/// <param name="TypeText"></param>
/// <param name="ErrorCode"></param>
/// <param name="ErrorMessage"></param>
/// <param name="Data"></param>
public record Model_Result<T>(string TypeText, int? ErrorCode, string? ErrorMessage, T? Data)
{
    public Model_Result() : this(ResultType.Success.ToString(), null, null, default) { }
    public Model_Result(T? data) : this(ResultType.SuccessReturnData.ToString(), null, null, data) { }
    public Model_Result(ResultType type) : this(type.ToString(), null, null, default) { }
    public Model_Result(ResultType type, int? errorCode, string? errorMessage) : this(type.ToString(), errorCode, errorMessage, default) { }
    public Model_Result(AuthenticationErrorCodes warning) : this(ResultType.Authentification.ToString(), (int) warning, warning.ToString(), default) { }
}

[Serializable]
public class RequestException(ResultType type, int? code = null, string? message = null, Exception? inner = null) : Exception(message, inner)
{
    public ResultType Type { get; set; } = type;
    public int? Code { get; set; } = code;
}

public enum ResultType
{
    Success = 0,
    SuccessReturnData = 1,
    GeneralError = 2,
    Authentification = 3,
    DataIsInvalid = 4,
    NoDataFound = 5,
    MissingRight = 6,
}