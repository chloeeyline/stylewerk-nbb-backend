namespace StyleWerk.NBB.Models;


public record ShareTypes(bool Group, bool Public, bool Directly);

/// <summary>
/// The default Model to always wrap the results of a requests from the frontend
/// </summary>
public record Model_Result<T>(int Code, ResultCodes CodeName, T? Data)
{
    public Model_Result() : this((int) ResultCodes.Success, ResultCodes.Success, default) { }
    public Model_Result(T? data) : this((int) ResultCodes.SuccessReturnData, ResultCodes.SuccessReturnData, data) { }
    public Model_Result(ResultCodes code) : this((int) code, code, default) { }
    public Model_Result(ResultCodes code, T? data) : this((int) code, code, data) { }
}

[Serializable]
public class RequestException(ResultCodes Code, string? message = null, Exception? inner = null) : Exception(message, inner)
{
    public ResultCodes Code { get; set; } = Code;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ResultCodesResponseAttribute(params ResultCodes[] possibleCodes) : Attribute
{
    public ResultCodes[] PossibleCodes { get; } = possibleCodes;
}


public enum ResultCodes
{
    Success = 1000,
    SuccessReturnData = 1001,

    GeneralError = 1100,
    DataIsInvalid = 1101,
    NoDataFound = 1102,
    MissingRight = 1103,

    #region Authentication
    EmailInvalid = 1210,
    EmailAlreadyExists = 1211,
    UsernameInvalid = 1212,
    UnToShort = 1213,
    UnToLong = 1214,
    UnUsesInvalidChars = 1215,
    UsernameAlreadyExists = 1216,

    WrongStatusCode = 1220,
    StatusTokenNotFound = 1221,
    StatusTokenExpired = 1222,
    StatusTokenAlreadyRequested = 1223,
    EmailChangeCodeWrong = 1224,
    PendingChangeOpen = 1225,

    NoUserFound = 1230,
    RefreshTokenNotFound = 1231,
    RefreshTokenExpired = 1232,
    EmailIsNotVerified = 1233,
    PasswordResetWasRequested = 1234,

    // Password Errors
    PasswordInvalid = 1240,
    PwTooShort = 1241,
    PwHasNoLowercaseLetter = 1242,
    PwHasNoUppercaseLetter = 1243,
    PwHasNoNumber = 1244,
    PwHasNoSpecialChars = 1245,
    PwHasWhitespace = 1246,
    PwUsesInvalidChars = 1247,
    #endregion

    #region Share
    GroupNameAlreadyExists = 1301,
    DontOwnGroup = 1302,
    OnlyOwnerCanChangePublicity = 1303
    #endregion
}