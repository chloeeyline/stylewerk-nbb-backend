namespace StyleWerk.NBB.Authentication;

[Serializable]
public class AuthenticationException : Exception
{
    public AuthenticationWarning ErrorCode { get; set; }
    public AuthenticationException(AuthenticationWarning code, string message, Exception inner) : base(message, inner) => ErrorCode = code;
    public AuthenticationException(AuthenticationWarning code) : base() => ErrorCode = code;
}
