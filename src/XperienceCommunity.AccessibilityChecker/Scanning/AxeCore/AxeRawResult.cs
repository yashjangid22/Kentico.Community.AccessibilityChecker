namespace XperienceCommunity.AccessibilityChecker.Scanning.AxeCore
{
    /// <summary>
    /// Minimal POCOs matching the JSON shape returned by axe-core's <c>axe.run()</c>.
    /// Deserialized with case-insensitive property matching (axe-core emits camelCase).
    /// </summary>
    internal sealed class AxeRawResult
    {
        public List<AxeRawViolation> Violations { get; set; } = [];
    }

    internal sealed class AxeRawViolation
    {
        public string Id { get; set; } = "";
        public string Impact { get; set; } = "";
        public string Description { get; set; } = "";
        public List<AxeRawNode> Nodes { get; set; } = [];
    }

    internal sealed class AxeRawNode
    {
        public List<string> Target { get; set; } = [];
    }
}
