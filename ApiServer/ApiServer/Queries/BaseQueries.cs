using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Queries;

public abstract class BaseQueries
{
    public NbbContext DB { get; }
    public ApplicationUser CurrentUser { get; }

    protected BaseQueries(NbbContext dB, ApplicationUser currentUser)
    {
        DB = dB ?? throw new ArgumentNullException(nameof(dB));
        CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }
}
