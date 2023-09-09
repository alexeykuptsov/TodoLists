using System.Reactive.Disposables;

namespace TodoLists.Tests.Integration;

public class TestSetUpContext
{
    public string ProfileName { get; }
    public CompositeDisposable CompositeDisposable { get; }

    public TestSetUpContext(string profileName, CompositeDisposable compositeDisposable)
    {
        ProfileName = profileName;
        CompositeDisposable = compositeDisposable;
    }
}