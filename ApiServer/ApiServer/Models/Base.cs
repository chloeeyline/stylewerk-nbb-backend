﻿using StyleWerk.NBB.Database.User;

namespace ChaosFox.Models;

public record PagingList<T>(int Count, List<T> Items);

public record ApplicationUser(bool Instantiated, Guid ID, User_Login Login, User_Information Information, User_Right Right);

/// <summary>
/// The default Model to always wrap the results of a requests from the frontend
/// </summary>
/// <param name="Type"></param>
/// <param name="ErrorCode"></param>
/// <param name="Data"></param>
public record Model_Result(ResultType Type, int? ErrorCode, object? Data)
{
    public Model_Result() : this(ResultType.Success, null, null) { }
    public Model_Result(object? data) : this(ResultType.SuccessReturnData, null, data) { }
    public Model_Result(ResultType type) : this(type, null, null) { }
    public Model_Result(ResultType type, int? errorCode) : this(type, errorCode, null) { }
}

public enum ResultType
{
    Success,
    SuccessReturnData,
    Authentification,
    MissingRight,
    NoDataFound,
    NoDataSend,
    ParameterRequirements
}