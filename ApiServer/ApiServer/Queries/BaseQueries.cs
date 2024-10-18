using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Queries;

public abstract class BaseQueries(NbbContext dB, ApplicationUser currentUser)
{
    public NbbContext DB { get; } = dB ?? throw new ArgumentNullException(nameof(dB));
    public ApplicationUser CurrentUser { get; } = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
}
