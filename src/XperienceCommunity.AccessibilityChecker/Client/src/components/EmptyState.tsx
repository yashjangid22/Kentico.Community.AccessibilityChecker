import React from "react";
import { Colors, Headline, HeadlineSize, Icon, LayoutAlignment, Spacing, Stack } from "@kentico/xperience-admin-components";

export const EmptyState = () => (
    <div style={{ textAlign: "center", padding: "48px 0" }}>
        <Stack align={LayoutAlignment.Center} spacing={Spacing.M}>
            <span style={{ color: Colors.IconLowEmphasis, fontSize: "32px" }}>
                <Icon name="xp-clipboard-checklist" />
            </span>
            <Headline size={HeadlineSize.M}>No scan results yet.</Headline>
            <p style={{ color: Colors.TextLowEmphasis, margin: 0 }}>Enter a URL above.</p>
        </Stack>
    </div>
);
