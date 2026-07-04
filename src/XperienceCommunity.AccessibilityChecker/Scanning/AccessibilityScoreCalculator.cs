namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    /// <summary>
    /// Deducts points once per violated rule (not per affected element), so a single
    /// widespread issue doesn't tank the score to 0 on its own.
    /// </summary>
    public static class AccessibilityScoreCalculator
    {
        public const int CriticalWeight = 10;
        public const int SeriousWeight = 7;
        public const int ModerateWeight = 3;
        public const int MinorWeight = 1;

        public static int Calculate(int criticalRuleCount, int seriousRuleCount, int moderateRuleCount, int minorRuleCount)
        {
            var deduction =
                criticalRuleCount * CriticalWeight +
                seriousRuleCount * SeriousWeight +
                moderateRuleCount * ModerateWeight +
                minorRuleCount * MinorWeight;

            return Math.Max(0, 100 - deduction);
        }
    }
}
