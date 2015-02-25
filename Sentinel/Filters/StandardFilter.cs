namespace Sentinel.Filters
{
    using Interfaces;
    using Sentinel.Interfaces;

    public class StandardFilter : Filter, IStandardDebuggingFilter
    {
        public StandardFilter()
        {
        }

        public StandardFilter(string name, LogEntryField field, string pattern)
            : base(name, field, pattern)
        {
        }
    }
}