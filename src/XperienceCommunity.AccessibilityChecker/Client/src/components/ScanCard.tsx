import React from "react";
import {
    Box,
    Button,
    ButtonColor,
    ButtonSize,
    Callout,
    CalloutPlacementType,
    CalloutType,
    Card,
    Colors,
    LayoutAlignment,
    Spacing,
    Spinner,
    Stack,
} from "@kentico/xperience-admin-components";

import { ScanCardState } from "../types";
import { SEVERITY_ORDER } from "../severity";
import { IssueGroup } from "./IssueGroup";
import { ScoreBadge } from "./ScoreBadge";

interface ScanCardProps {
    state: ScanCardState;
    onRescan: () => void;
    onDelete: () => void;
}

function formatTimestamp(timestamp: string): string {
    try {
        return new Date(timestamp).toLocaleString();
    } catch {
        return timestamp;
    }
}

function getErrorHeadline(errorCode: string, hasPriorResult: boolean): string {
    if (errorCode === "AccessRestricted") {
        return "Access restricted by this site";
    }
    return hasPriorResult ? "Re-scan failed" : "Scan failed";
}

export const ScanCard = ({ state, onRescan, onDelete }: ScanCardProps) => {
    const { url, isScanning, result, error } = state;
    const isFirstScan = isScanning && !result && !error;

    const footer = (
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", width: "100%" }}>
            <Button
                label="Re-scan"
                icon="xp-magnifier"
                color={ButtonColor.Secondary}
                size={ButtonSize.S}
                inProgress={isScanning && !!result}
                disabled={isScanning}
                onClick={onRescan}
            />
            <Button
                icon="xp-bin"
                color={ButtonColor.Tertiary}
                size={ButtonSize.S}
                destructive
                disabled={isScanning}
                onClick={onDelete}
                title="Delete this result"
            />
        </div>
    );

    return (
        <Card
            headline={`Scanning: ${url}`}
            description={result ? `Last scanned ${formatTimestamp(result.timestamp)}` : undefined}
            footer={footer}
        >
            {isFirstScan && (
                <div style={{ padding: "24px 0", textAlign: "center" }}>
                    <Stack align={LayoutAlignment.Center} spacing={Spacing.M}>
                        <Spinner />
                        <span>Scanning {url}…</span>
                    </Stack>
                </div>
            )}

            {!isFirstScan && error && (
                <Box spacingBottom={Spacing.M}>
                    <Callout
                        type={CalloutType.FriendlyWarning}
                        placement={CalloutPlacementType.OnPaper}
                        headline={getErrorHeadline(error.code, !!result)}
                        subheadline={error.message}
                    />
                </Box>
            )}

            {!isFirstScan && result && (
                <div style={{ opacity: isScanning ? 0.6 : 1 }}>
                    <Box spacingBottom={Spacing.M}>
                        <ScoreBadge score={result.score} />
                    </Box>
                    {SEVERITY_ORDER.every(severity => result.issuesBySeverity[severity].length === 0) ? (
                        <p style={{ color: Colors.SuccessText }}>No issues found! Great job.</p>
                    ) : (
                        <Stack spacing={Spacing.L}>
                            {SEVERITY_ORDER.map(severity => (
                                <IssueGroup key={severity} severity={severity} issues={result.issuesBySeverity[severity]} />
                            ))}
                        </Stack>
                    )}
                </div>
            )}
        </Card>
    );
};
