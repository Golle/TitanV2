using Titan.Application;

namespace Titan.Runners;
internal interface IRunner
{
    static abstract IRunner Create();
    void Init(IApp app);
    bool RunOnce();
}
