namespace StyleWerk.NBB.Authentication;

public enum AuthenticationErrorCodes
{
    ModelIncorrect = 1001,

    EmailInvalid = 1010,
    EmailAlreadyExists = 1011,
    UsernameInvalid = 1012,
    UnToShort = 1013,
    UnToLong = 1014,
    UnUsesInvalidChars = 1015,
    UsernameAlreadyExists = 1016,

    WrongStatusCode = 1020,
    StatusTokenNotFound = 1021,
    StatusTokenExpired = 1022,
    StatusTokenAlreadyRequested = 1023,
    EmailChangeCodeWrong = 1024,
    PendingChangeOpen = 1025,

    NoUserFound = 1030,
    RefreshTokenNotFound = 1031,
    RefreshTokenExpired = 1032,
    EmailIsNotVerified = 1033,
    PasswordResetWasRequested = 1034,

    // Password Errors
    PasswordInvalid = 1040,
    PwTooShort = 1041,
    PwHasNoLowercaseLetter = 1042,
    PwHasNoUppercaseLetter = 1043,
    PwHasNoNumber = 1044,
    PwHasNoSpecialChars = 1045,
    PwHasWhitespace = 1046,
    PwUsesInvalidChars = 1047,
}