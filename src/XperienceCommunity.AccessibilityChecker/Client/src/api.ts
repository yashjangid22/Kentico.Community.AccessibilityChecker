import { ScanApiError, ScanResult } from "./types";

export async function scanUrl(url: string): Promise<ScanResult> {
    const response = await fetch("/api/accessibility/scan", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url }),
    });

    if (!response.ok) {
        const error: ScanApiError = await response.json().catch(() => ({
            code: "ScanFailed",
            message: `Scan failed with status ${response.status}.`,
        }));
        throw error;
    }

    return response.json();
}

export async function getAllScans(): Promise<ScanResult[]> {
    const response = await fetch("/api/accessibility/scans");

    if (!response.ok) {
        throw new Error(`Failed to load past scans (status ${response.status}).`);
    }

    return response.json();
}

export async function deleteScan(url: string): Promise<void> {
    const response = await fetch(`/api/accessibility/scan?url=${encodeURIComponent(url)}`, {
        method: "DELETE",
    });

    if (!response.ok) {
        const error: ScanApiError = await response.json().catch(() => ({
            code: "ScanFailed",
            message: `Delete failed with status ${response.status}.`,
        }));
        throw error;
    }
}
