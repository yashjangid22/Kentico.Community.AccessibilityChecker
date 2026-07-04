import React, { useState } from "react";
import { Button, ButtonColor, ButtonType, Input, Spacing } from "@kentico/xperience-admin-components";

interface ScanFormProps {
    onScan: (url: string) => void;
    disabled: boolean;
}

function isValidHttpUrl(value: string): boolean {
    try {
        const parsed = new URL(value);
        return parsed.protocol === "http:" || parsed.protocol === "https:";
    } catch {
        return false;
    }
}

export const ScanForm = ({ onScan, disabled }: ScanFormProps) => {
    const [url, setUrl] = useState("");
    const [formError, setFormError] = useState<string | undefined>(undefined);

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();
        const trimmed = url.trim();

        if (!isValidHttpUrl(trimmed)) {
            setFormError("Enter a valid URL starting with http:// or https://");
            return;
        }

        setFormError(undefined);
        onScan(trimmed);
    };

    return (
        <form onSubmit={handleSubmit}>
            <div style={{ display: "flex", flexDirection: "row", alignItems: "flex-end", gap: Spacing.M }}>
                <div style={{ flexGrow: 1 }}>
                    <Input
                        label="URL to scan"
                        placeholder="https://yoursite.com/about-us"
                        value={url}
                        onChange={e => setUrl(e.target.value)}
                        invalid={!!formError}
                        validationMessage={formError}
                    />
                </div>
                <Button
                    label="Scan"
                    icon="xp-magnifier"
                    color={ButtonColor.Primary}
                    type={ButtonType.Submit}
                    inProgress={disabled}
                    disabled={disabled}
                />
            </div>
        </form>
    );
};
