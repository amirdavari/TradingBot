import { useEffect, useRef, useState } from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import {
    getConnection,
    startConnection,
    getConnectionState,
    TradingHubMethods,
} from '../signalr/connection';
import type { ReplayState, PaperTrade, Account, ScanResult } from '../models';

// Singleton connection - initialized once outside of React
const connection = getConnection();

// Connection state tracking
let connectionInitialized = false;

// Start connection immediately when module loads (only once)
if (!connectionInitialized) {
    connectionInitialized = true;
    startConnection()
        .then(() => console.log('[SignalR] Initial connection established, state:', connection.state))
        .catch((err) => console.error('[SignalR] Initial connection failed:', err));
}

/**
 * Hook that provides the SignalR connection and handles lifecycle.
 * Connection is started automatically when the module loads.
 */
export function useSignalR() {
    const [connectionState, setConnectionState] = useState<HubConnectionState>(
        getConnectionState()
    );

    useEffect(() => {
        // Update state when connection state changes
        const updateState = () => {
            setConnectionState(connection.state);
        };

        // Set initial state
        updateState();

        connection.onreconnecting(updateState);
        connection.onreconnected(updateState);
        connection.onclose(updateState);

        // Note: Connection is started at module level, no need to call startConnection here
    }, []);

    return {
        connection,
        connectionState,
        isConnected: connectionState === HubConnectionState.Connected,
    };
}

/**
 * Hook that listens for replay state updates via SignalR.
 * Handler is registered immediately on the singleton connection.
 */
export function useSignalRReplayState(onStateUpdate?: (state: ReplayState) => void) {
    const callbackRef = useRef(onStateUpdate);

    // Keep callback ref current
    useEffect(() => {
        callbackRef.current = onStateUpdate;
    }, [onStateUpdate]);

    useEffect(() => {
        const handler = (state: ReplayState) => {
            console.log('[SignalR] Received replay state:', state);
            callbackRef.current?.(state);
        };

        connection.on(TradingHubMethods.ReceiveReplayState, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveReplayState, handler);
        };
    }, []); // Empty deps - register once on mount
}

/**
 * Hook that listens for trade closed events via SignalR.
 */
export function useSignalRTradeClosed(
    onTradeClosed?: (data: { trade: PaperTrade; reason: string }) => void
) {
    const callbackRef = useRef(onTradeClosed);

    useEffect(() => {
        callbackRef.current = onTradeClosed;
    }, [onTradeClosed]);

    useEffect(() => {
        const handler = (data: { trade: PaperTrade; reason: string }) => {
            console.log('[SignalR] Trade closed:', data);
            callbackRef.current?.(data);
        };

        connection.on(TradingHubMethods.ReceiveTradeClosed, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveTradeClosed, handler);
        };
    }, []);
}

/**
 * Hook that listens for account updates via SignalR.
 */
export function useSignalRAccountUpdate(onAccountUpdate?: (account: Account) => void) {
    const callbackRef = useRef(onAccountUpdate);

    useEffect(() => {
        callbackRef.current = onAccountUpdate;
    }, [onAccountUpdate]);

    useEffect(() => {
        const handler = (account: Account) => {
            console.log('[SignalR] Account update:', account);
            callbackRef.current?.(account);
        };

        connection.on(TradingHubMethods.ReceiveAccountUpdate, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveAccountUpdate, handler);
        };
    }, []);
}

/**
 * Hook that listens for trade updates via SignalR.
 */
export function useSignalRTradeUpdate(onTradeUpdate?: (trade: PaperTrade) => void) {
    const callbackRef = useRef(onTradeUpdate);

    useEffect(() => {
        callbackRef.current = onTradeUpdate;
    }, [onTradeUpdate]);

    useEffect(() => {
        const handler = (trade: PaperTrade) => {
            console.log('[SignalR] Trade update:', trade);
            callbackRef.current?.(trade);
        };

        connection.on(TradingHubMethods.ReceiveTradeUpdate, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveTradeUpdate, handler);
        };
    }, []);
}

/**
 * Hook that listens for scanner results via SignalR.
 * Backend pushes results every 10 seconds during replay.
 */
export function useSignalRScanResults(onScanResults?: (results: ScanResult[]) => void) {
    const callbackRef = useRef(onScanResults);

    useEffect(() => {
        callbackRef.current = onScanResults;
    }, [onScanResults]);

    useEffect(() => {
        const handler = (results: ScanResult[]) => {
            console.log('[SignalR] Scanner results:', results.length);
            callbackRef.current?.(results);
        };

        connection.on(TradingHubMethods.ReceiveScanResults, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveScanResults, handler);
        };
    }, []);
}

/**
 * Hook that listens for chart refresh notifications via SignalR.
 * Backend pushes refresh signal during replay when time advances.
 */
export function useSignalRChartRefresh(onChartRefresh?: (data: { symbols: string[] }) => void) {
    const callbackRef = useRef(onChartRefresh);

    useEffect(() => {
        callbackRef.current = onChartRefresh;
    }, [onChartRefresh]);

    useEffect(() => {
        const handler = (data: { symbols: string[] }) => {
            console.log('[SignalR] Chart refresh:', data);
            callbackRef.current?.(data);
        };

        connection.on(TradingHubMethods.ReceiveChartRefresh, handler);

        return () => {
            connection.off(TradingHubMethods.ReceiveChartRefresh, handler);
        };
    }, []);
}
