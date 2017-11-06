
using LanguageExt;

namespace workshop_structuredlogging
{
    public interface IRequestRegistery
    {
        Result<bool> RequestExists(RequestId requestId);
        Result<Unit> StoreRequest(RequestId requestId);
    }
}
