using Mix.Core.Entity;

namespace Mix.Core.Contexts;

public class PrintContext
{
    public PrintContext(BatchInfo batchInfo)
    {
        BatchInfo = batchInfo;
    }

    public BatchInfo BatchInfo { get; internal set; }

    public Container Container { get; internal set; }
    public Weight Weight { get; internal set; }
}
