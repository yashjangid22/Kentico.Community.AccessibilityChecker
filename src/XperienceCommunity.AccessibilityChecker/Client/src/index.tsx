import React, { useEffect, useState } from "react";
import { Box, Headline, HeadlineSize, Spacing, Stack } from "@kentico/xperience-admin-components";

import { deleteScan, getAllScans, scanUrl } from "./api";
import { ScanApiError, ScanCardState } from "./types";
import { ScanForm } from "./components/ScanForm";
import { ScanCard } from "./components/ScanCard";
import { EmptyState } from "./components/EmptyState";

export const TabTemplate = () => {
    const [cardsByUrl, setCardsByUrl] = useState<Record<string, ScanCardState>>({});
    const [cardOrder, setCardOrder] = useState<string[]>([]);
    const [historyLoaded, setHistoryLoaded] = useState(false);

    useEffect(() => {
        let cancelled = false;

        getAllScans()
            .then(results => {
                if (cancelled) return;
                const byUrl: Record<string, ScanCardState> = {};
                const order: string[] = [];
                for (const result of results) {
                    byUrl[result.url] = { url: result.url, isScanning: false, result, error: undefined };
                    order.push(result.url);
                }
                setCardsByUrl(byUrl);
                setCardOrder(order);
            })
            .catch(() => {
                // Non-critical: if history can't be loaded, just start with an empty list,
                // same as before this feature existed.
            })
            .finally(() => {
                if (!cancelled) setHistoryLoaded(true);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    const runScan = async (url: string) => {
        setCardsByUrl(prev => ({
            ...prev,
            [url]: { ...prev[url], url, isScanning: true },
        }));
        setCardOrder(prev => (prev.includes(url) ? prev : [url, ...prev]));

        try {
            const result = await scanUrl(url);
            setCardsByUrl(prev => ({ ...prev, [url]: { url, isScanning: false, result, error: undefined } }));
        } catch (err) {
            const apiError = err as ScanApiError;
            setCardsByUrl(prev => ({
                ...prev,
                [url]: { ...prev[url], url, isScanning: false, error: apiError },
            }));
        }
    };

    const deleteCard = async (url: string) => {
        try {
            await deleteScan(url);
            setCardOrder(prev => prev.filter(entry => entry !== url));
            setCardsByUrl(prev => {
                const next = { ...prev };
                delete next[url];
                return next;
            });
        } catch (err) {
            const apiError = err as ScanApiError;
            setCardsByUrl(prev => ({
                ...prev,
                [url]: { ...prev[url], url, error: apiError },
            }));
        }
    };

    const isAnyScanInFlight = Object.values(cardsByUrl).some(card => card.isScanning);

    return (
        <Box spacing={Spacing.XL}>
            <Box spacingBottom={Spacing.L}>
                <Headline size={HeadlineSize.L}>Accessibility checker</Headline>
                <p>Scans a page for WCAG 2.1 accessibility issues using axe-core.</p>
            </Box>
            <ScanForm onScan={runScan} disabled={isAnyScanInFlight} />
            <Box spacingTop={Spacing.XL}>
                {cardOrder.length === 0 ? (
                    historyLoaded && <EmptyState />
                ) : (
                    <Stack spacing={Spacing.L}>
                        {cardOrder.map(url => (
                            <ScanCard
                                key={url}
                                state={cardsByUrl[url]}
                                onRescan={() => runScan(url)}
                                onDelete={() => deleteCard(url)}
                            />
                        ))}
                    </Stack>
                )}
            </Box>
        </Box>
    );
};

export default TabTemplate;
