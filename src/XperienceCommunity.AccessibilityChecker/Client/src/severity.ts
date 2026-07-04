import { Colors } from "@kentico/xperience-admin-components";

import { Severity } from "./types";

export interface SeverityStyle {
    label: string;
    background: Colors;
    accent: Colors;
}

export const SEVERITY_ORDER: Severity[] = ["critical", "serious", "moderate", "minor"];

// Uses the dedicated BackgroundTag* palette (rather than the *BackgroundLowEmphasis semantic
// tokens) for "serious"/"minor": the low-emphasis pastel backgrounds don't give the Tag
// component's text enough contrast to read. BackgroundTag* is the palette Tag is designed
// to pair with, and gives four visually distinct, readable severities (red -> rose -> amber
// -> yellow) instead of two color families repeated at two emphasis levels.
export const SEVERITY_STYLES: Record<Severity, SeverityStyle> = {
    critical: { label: "Critical", background: Colors.AlertBackgroundHighEmphasis, accent: Colors.AlertText },
    serious: { label: "Serious", background: Colors.BackgroundTagRose, accent: Colors.TextHighEmphasis },
    moderate: { label: "Moderate", background: Colors.WarningBackgroundHighEmphasis, accent: Colors.WarningText },
    minor: { label: "Minor", background: Colors.BackgroundTagYellow, accent: Colors.TextHighEmphasis },
};

export interface ScoreStyle {
    color: Colors;
}

export function getScoreStyle(score: number): ScoreStyle {
    if (score >= 90) return { color: Colors.SuccessText };
    if (score >= 60) return { color: Colors.WarningText };
    return { color: Colors.AlertText };
}
