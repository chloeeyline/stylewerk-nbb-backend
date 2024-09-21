namespace StyleWerk.NBB.Authentication;

public enum AuthenticationErrorCodes
{
    ModelIncorrect = 1,

    EmailInvalid = 10,
    EmailAlreadyExists = 11,
    UsernameInvalid = 12,
    UnToShort = 13,
    UnToLong = 14,
    UnUsesInvalidChars = 15,
    UsernameAlreadyExists = 16,

    WrongStatusCode = 20,
    StatusTokenNotFound = 21,
    StatusTokenExpired = 22,
    StatusTokenAlreadyRequested = 23,

    NoUserFound = 50,
    RefreshTokenNotFound = 51,
    RefreshTokenExpired = 52,
    EmailIsNotVerified = 53,
    PasswordResetWasRequested = 54,

    // Password Errors
    PasswordInvalid = 100,
    PwTooShort = 101,
    PwHasNoLowercaseLetter = 102,
    PwHasNoUppercaseLetter = 103,
    PwHasNoNumber = 104,
    PwHasNoSpecialChars = 105,
    PwHasWhitespace = 106,
    PwUsesInvalidChars = 107,
}