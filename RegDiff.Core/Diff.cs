namespace RegDiff.Core
{
    public struct Diff
    {
        public string BaseVal { get; set; }
        public string ChangedVal { get; set; }
        public DiffType DiffType { get; set; }
    }
}