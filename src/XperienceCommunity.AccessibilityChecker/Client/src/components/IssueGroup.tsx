import React, { useState } from "react";
import { Colors, LinkButton, Spacing, Stack, Tag } from "@kentico/xperience-admin-components";

import { AccessibilityIssue, Severity } from "../types";
import { SEVERITY_STYLES } from "../severity";

interface IssueRowProps {
    issue: AccessibilityIssue;
}

const IssueRow = ({ issue }: IssueRowProps) => {
    const [expanded, setExpanded] = useState(false);

    return (
        <div style={{ padding: "10px 0", borderBottom: `1px solid ${Colors.DividerDefault}` }}>
            <div style={{ display: "flex", flexWrap: "wrap", alignItems: "flex-start", justifyContent: "space-between", gap: Spacing.S }}>
                <div style={{ flex: "1 1 260px" }}>
                    <strong>{issue.rule}</strong>
                    <span style={{ color: Colors.TextLowEmphasis }}>
                        {" "}
                        — {issue.description} ({issue.affectedElementCount} element
                        {issue.affectedElementCount === 1 ? "" : "s"})
                    </span>
                </div>
                {issue.selectors.length > 0 && (
                    <div style={{ flexShrink: 0 }}>
                        <LinkButton
                            label={expanded ? "Hide affected elements" : "Show affected elements"}
                            onClick={() => setExpanded(v => !v)}
                        />
                    </div>
                )}
            </div>
            {expanded && (
                <ul style={{ margin: "8px 0 0", paddingLeft: "20px" }}>
                    {issue.selectors.map((selector, index) => (
                        <li key={`${selector}-${index}`}>
                            <code style={{ color: Colors.TextLowEmphasis, fontSize: "12px" }}>{selector}</code>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

interface IssueGroupProps {
    severity: Severity;
    issues: AccessibilityIssue[];
}

export const IssueGroup = ({ severity, issues }: IssueGroupProps) => {
    if (issues.length === 0) {
        return null;
    }

    const style = SEVERITY_STYLES[severity];

    return (
        <Stack spacing={Spacing.S}>
            <div style={{ alignSelf: "flex-start" }}>
                <Tag label={`${style.label} (${issues.length})`} background={{ color: style.background }} />
            </div>
            <div>
                {issues.map(issue => (
                    <IssueRow key={issue.rule} issue={issue} />
                ))}
            </div>
        </Stack>
    );
};
