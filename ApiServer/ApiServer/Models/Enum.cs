namespace StyleWerk.NBB.Models;

[Flags]
public enum ShareType
{
    /// <summary>
    /// When it is your own Entry
    /// </summary>
    Own,
    /// <summary>
    /// When it is shared to you as a User directly
    /// </summary>
    Direcly,
    /// <summary>
    /// When you have access to it because it got shared to a group you are in
    /// </summary>
    Group,
    /// <summary>
    /// When everybody can view it
    /// </summary>
    Public
}

public enum ItemState
{
    None = 0,
    Modified = 1,
    Deleted = 2,
    Added = 3,
}