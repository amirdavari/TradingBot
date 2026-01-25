import * as signalR from '@microsoft/signalr';
import { API_CONFIG } from '../api/config';

// Hub URL - derived from API base URL
const HUB_URL = `${API_CONFIG.baseURL}/hubs/trading`;

// Singleton connection instance
let connection: signalR.HubConnection | null = null;
let connectionPromise: Promise<void> | null = null;

/**
 * Creates or returns the existing SignalR connection.
 * Uses singleton pattern to ensure only one connection exists.
 */
export function getConnection(): signalR.HubConnection {
    if (!connection) {
        console.log('[SignalR] Creating connection to:', HUB_URL);
        connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                withCredentials: true // Required for CORS with credentials
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Log connection state changes
        connection.onreconnecting((error) => {
            console.log('[SignalR] Reconnecting...', error);
        });

        connection.onreconnected((connectionId) => {
            console.log('[SignalR] Reconnected:', connectionId);
        });

        connection.onclose((error) => {
            console.log('[SignalR] Connection closed:', error);
            connectionPromise = null;
        });
    }
    return connection;
}

/**
 * Starts the SignalR connection if not already connected.
 * Safe to call multiple times - will reuse existing connection.
 */
export async function startConnection(): Promise<void> {
    const conn = getConnection();

    console.log('[SignalR] startConnection called, current state:', conn.state);

    if (conn.state === signalR.HubConnectionState.Connected) {
        console.log('[SignalR] Already connected');
        return; // Already connected
    }

    if (conn.state === signalR.HubConnectionState.Connecting && connectionPromise) {
        console.log('[SignalR] Connection already in progress');
        return connectionPromise; // Connection in progress
    }

    console.log('[SignalR] Starting new connection to:', HUB_URL);
    connectionPromise = conn.start()
        .then(() => {
            console.log('[SignalR] Connected successfully to', HUB_URL, '| State:', conn.state);
        })
        .catch((err) => {
            console.error('[SignalR] Connection failed:', err);
            connectionPromise = null;
            throw err;
        });

    return connectionPromise;
}

/**
 * Stops the SignalR connection.
 */
export async function stopConnection(): Promise<void> {
    if (connection) {
        await connection.stop();
        console.log('[SignalR] Disconnected');
    }
}

/**
 * Returns the current connection state.
 */
export function getConnectionState(): signalR.HubConnectionState {
    return connection?.state ?? signalR.HubConnectionState.Disconnected;
}

// Re-export types and constants for convenience
export { HubConnectionState } from '@microsoft/signalr';

/**
 * Client method names that the server can invoke.
 * Must match TradingHubMethods in the backend.
 */
export const TradingHubMethods = {
    ReceiveReplayState: 'ReceiveReplayState',
    ReceiveTradeUpdate: 'ReceiveTradeUpdate',
    ReceiveTradeClosed: 'ReceiveTradeClosed',
    ReceiveAccountUpdate: 'ReceiveAccountUpdate',
    ReceiveScanResults: 'ReceiveScanResults',
    ReceiveChartRefresh: 'ReceiveChartRefresh',
} as const;
