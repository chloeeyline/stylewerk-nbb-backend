using StyleWerk.NBB.Authentication;

namespace StyleWerk.NBB.Models;

public record PagingList<T>(int Count, int Page, int MaxPage, int PerPage, List<T> Items);

public record ShareTypes(bool Own, bool GroupShared, bool Public, bool DirectlyShared);
public record Model_SharedItem(Guid ID, string SharedFrom, bool FromGroup, Guid? GroupID, string? GroupName, bool CanShare, bool CanEdit, bool CanDelete);

/// <summary>
/// The default Model to always wrap the results of a requests from the frontend
/// </summary>
/// <param name="Type"></param>
/// <param name="TypeText"></param>
/// <param name="ErrorCode"></param>
/// <param name="ErrorMessage"></param>
/// <param name="Data"></param>
public record Model_Result<T>(ResultType Type, string TypeText, int? ErrorCode, string? ErrorMessage, T? Data)
{
    public Model_Result() : this(ResultType.Success, ResultType.Success.ToString(), null, null, default) { }
    public Model_Result(T? data) : this(ResultType.SuccessReturnData, ResultType.SuccessReturnData.ToString(), null, null, data) { }
    public Model_Result(ResultType type) : this(type, type.ToString(), null, null, default) { }
    public Model_Result(ResultType type, int? errorCode, string? errorMessage) : this(type, type.ToString(), errorCode, errorMessage, default) { }
    public Model_Result(AuthenticationErrorCodes warning) : this(ResultType.Authentification, ResultType.Authentification.ToString(), (int) warning, warning.ToString(), default) { }
}

[Serializable]
public class RequestException : Exception
{
    public ResultType ErrorCode { get; set; }
    public RequestException(ResultType code) : base() => ErrorCode = code;
    public RequestException(ResultType code, string message) : base(message) => ErrorCode = code;
    public RequestException(ResultType code, string message, Exception inner) : base(message, inner) => ErrorCode = code;
}

public enum ResultType
{
    Success,
    SuccessReturnData,
    GeneralError,
    Authentification,
    NoDataFound,
    MissingRight,
    DataIsInvalid,
}