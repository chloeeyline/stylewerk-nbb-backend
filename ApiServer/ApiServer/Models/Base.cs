﻿using System.Runtime.CompilerServices;

namespace StyleWerk.NBB.Models;

/// <summary>
/// Used to determine how many items exists for the given fitler
/// </summary>
/// <param name="Count">total count</param>
/// <param name="Page">current page</param>
/// <param name="MaxPage">max number of page</param>
/// <param name="PerPage">how many items should be shown per page</param>
public record Paging(int Count, int Page, int MaxPage, int PerPage);

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
public class RequestException(ResultCodes Code, string? message = null, Exception? inner = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0) : Exception(CreateMessage(Code, message, memberName, lineNumber), inner)
{
    public ResultCodes Code { get; set; } = Code;

    /// <summary>
    /// Used to throw a new exception based on the given result code
    /// </summary>
    /// <param name="code">are defined in an enum</param>
    /// <param name="message">additional message for the user</param>
    /// <param name="memberName">shows the member</param>
    /// <param name="lineNumber">shows the number of the line in which the error was thrown</param>
    /// <returns></returns>
    private static string CreateMessage(ResultCodes code,
        string? message = null,
        string memberName = "",
        int lineNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(message))
        { message = string.Empty; }
        return $"Code: {code}, Method: {memberName}, Line: {lineNumber}, Message: \"{message}\"";
    }
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
    YouDontOwnTheData = 1104,
    NameMustBeUnique = 1105,
    UserMustBeAdmin = 1106,

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

    CantShareWithYourself = 1301,
    TemplateDoesntMatch = 1302,
}