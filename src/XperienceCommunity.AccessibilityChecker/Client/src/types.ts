export type Severity = "critical" | "serious" | "moderate" | "minor";

export interface AccessibilityIssue {
    rule: string;
    description: string;
    affectedElementCount: number;
    selectors: string[];
}

export interface IssuesBySeverity {
    critical: AccessibilityIssue[];
    serious: AccessibilityIssue[];
    moderate: AccessibilityIssue[];
    minor: AccessibilityIssue[];
}

export interface ScanResult {
    url: string;
    score: number;
    timestamp: string;
    issuesBySeverity: IssuesBySeverity;
}

export type ScanErrorCode = "InvalidUrl" | "UnreachablePage" | "Timeout" | "AccessRestricted" | "ScanFailed";

export interface ScanApiError {
    code: ScanErrorCode;
    message: string;
}

export interface ScanCardState {
    url: string;
    isScanning: boolean;
    result?: ScanResult;
    error?: ScanApiError;
}
