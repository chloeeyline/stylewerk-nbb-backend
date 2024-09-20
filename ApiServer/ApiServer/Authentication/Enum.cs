namespace StyleWerk.NBB.Authentication;

public enum AuthenticationWarning
{
	None = 0,
	ModelIsNull = 1,
	ModelIncorrect = 2,

	EmailInvalid = 10,
	UsernameInvalid = 11,
	PasswordInvalid = 12,
	EmailAlreadyExists = 13,
	UsernameAlreadyExists = 14,

	WrongStatusCode = 15,
	StatusTokenNotFound = 16,
	StatusTokenExpired = 17,

	NoUserFound = 50,
	EmailIsNotVerified = 51,
	PasswordResetWasRequested = 52,
	WrongPassword = 53,
	RefreshTokenNotFound = 54,
	RefreshTokenExpired = 55
}

[Flags]
public enum PasswordError
{
	None = 0,  // Represents no errors.
	TooShort = 1 << 0,  // 2^0 = 1
	HasNoLowercaseLetter = 1 << 1,  // 2^1 = 2
	HasNoUppercaseLetter = 1 << 2,  // 2^2 = 4
	HasNoNumber = 1 << 3,  // 2^3 = 8
	HasNoSpecialChars = 1 << 4,  // 2^4 = 16
	HasWhitespace = 1 << 5,  // 2^5 = 32
	UsesInvalidChars = 1 << 6   // 2^6 = 64
}
