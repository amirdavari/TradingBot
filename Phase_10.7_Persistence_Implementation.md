# Phase 10.7 - Simulator Persistence Implementation

## Overview
Implemented persistence layer for simulator/replay configuration to survive backend restarts.

## What Was Implemented

### 1. Database Entity (`API/Models/ReplayStateEntity.cs`)
Created a new entity to store replay state in the database:
- **Singleton pattern** with fixed `Id = 1`
- Stores: Mode, ReplayStartTime, CurrentTime, Speed, IsRunning

```csharp
public class ReplayStateEntity
{
    public int Id { get; set; } = 1; // Singleton
    public DateTime ReplayStartTime { get; set; }
    public DateTime CurrentTime { get; set; }
    public double Speed { get; set; }
    public bool IsRunning { get; set; }
    public MarketMode Mode { get; set; }
}
```

### 2. Database Context Updates (`API/Data/ApplicationDbContext.cs`)
- Added `DbSet<ReplayStateEntity> ReplayStates`
- Configured entity with `ValueGeneratedNever()` for singleton pattern
- Set all properties as required

### 3. Service Layer Updates (`API/Services/MarketTimeProvider.cs`)
Enhanced `MarketTimeProvider` with persistence logic:

#### Constructor Changes
- Added `IServiceProvider` dependency injection
- Enables creating scoped `DbContext` instances

#### New Methods
- **`LoadStateFromDatabase()`**: Loads persisted state on initialization
  - Falls back to default (Live mode) if no state exists
  - Logs loaded state for debugging
  
- **`SaveStateToDatabase()`**: Persists current state after every change
  - Creates scoped `DbContext` to avoid singleton issues
  - Uses Add or Update pattern
  - Logs save operations

#### Modified Methods
- **`SetMode()`**: Calls `SaveStateToDatabase()` after mode changes
- **`UpdateReplayState()`**: Calls `SaveStateToDatabase()` after state updates

## How It Works

### On Backend Startup
1. `MarketTimeProvider` constructor is called
2. `LoadStateFromDatabase()` attempts to load existing state
3. If found: Applies persisted configuration
4. If not found: Uses default values (Live mode)

### On Configuration Change
1. User changes replay settings (mode, speed, time, etc.)
2. `MarketTimeProvider` updates in-memory state
3. `SaveStateToDatabase()` automatically persists changes
4. Future restarts will load these settings

### Singleton Pattern
- Only one row in `ReplayStates` table (Id=1)
- EF Core uses "Add or Update" pattern:
  - First save: INSERT new row
  - Subsequent saves: UPDATE existing row

## Testing Results

### Test 1: Mode Persistence
✅ **Passed**
- Set mode to Replay
- Restarted backend
- Mode remained Replay (not defaulting to Live)

### Test 2: Speed Persistence
✅ **Passed**
- Set speed to 5x
- Restarted backend
- Speed remained 5x

### Test 3: Running State Persistence
✅ **Passed**
- Started replay (IsRunning=true)
- Restarted backend
- Replay continued running automatically
- Clock time continued from persisted value

### Test 4: Complete State Persistence
✅ **Passed**
All properties persisted correctly:
- Mode: Replay
- ReplayStartTime: 2026-01-17T15:11:03
- CurrentTime: Advanced correctly (clock continued)
- Speed: 5x
- IsRunning: true

## Log Evidence

### On First Start (No Persisted State)
```
SELECT "r"."Id", "r"."CurrentTime", "r"."IsRunning", "r"."Mode", "r"."ReplayStartTime", "r"."Speed"
FROM "ReplayStates" AS "r"
LIMIT 1

MarketTimeProvider initialized with mode: Live, CurrentTime: 01/17/2026 15:08:44
```

### On Configuration Change
```
SaveChanges completed for 'ApplicationDbContext' with 1 entities written to the database.
Saved replay state to database
```

### On Restart (With Persisted State)
```
Loaded replay state from database: Mode=Replay, Time=01/17/2026 15:11:03
MarketTimeProvider initialized with mode: Replay, CurrentTime: 01/17/2026 15:11:03
```

## Benefits

1. **User Experience**: Settings survive restarts, no re-configuration needed
2. **Development**: Simulator state preserved during debugging/restarts
3. **Reliability**: Replay simulations can be paused/resumed across sessions
4. **Consistency**: Clock time persists, maintaining simulation continuity

## Technical Highlights

- ✅ Singleton pattern for single-row configuration table
- ✅ Scoped DbContext usage from singleton service (avoids captive dependency)
- ✅ Automatic persistence on every state change
- ✅ Graceful fallback to defaults if no state exists
- ✅ Comprehensive logging for debugging

## Files Modified

1. **Created**: `API/Models/ReplayStateEntity.cs`
2. **Modified**: `API/Data/ApplicationDbContext.cs`
3. **Modified**: `API/Services/MarketTimeProvider.cs`

## Database Schema

```sql
CREATE TABLE "ReplayStates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ReplayStates" PRIMARY KEY,
    "ReplayStartTime" TEXT NOT NULL,
    "CurrentTime" TEXT NOT NULL,
    "Speed" REAL NOT NULL,
    "IsRunning" INTEGER NOT NULL,
    "Mode" INTEGER NOT NULL
);
```

## Conclusion

✅ **Phase 10.7 Persistence Feature: Complete**

The simulator now fully persists all configuration across backend restarts, providing a seamless experience for users testing trading strategies in replay mode.
