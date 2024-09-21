namespace StyleWerk.NBB.Authentication;

[Serializable]
public class AuthenticationException : Exception
{
    public AuthenticationErrorCodes ErrorCode { get; set; }
    public AuthenticationException(AuthenticationErrorCodes code, string message, Exception inner) : base(message, inner) => ErrorCode = code;
    public AuthenticationException(AuthenticationErrorCodes code) : base() => ErrorCode = code;
}
