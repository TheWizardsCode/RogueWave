Follow the global AGENTS.md in addition to the rules below. The local rules below take priority in the event of a conflict.

## Local Rules

- Whenever you are provided with or discover important context (specifications, designs, user-stories) ensure the information is added to the description of the relevant work item(s) OR create a new work item if none exist
- Whenever you create a planning document (PRD, spec, design doc) add references to the document in the description of any work item that is directly related to the document

## Architecture Notes for Agents

### Data Storage Architecture

Worklog uses **SQLite as the runtime source of truth** with an **ephemeral JSONL pattern** for Git sync:

- **SQLite** (`.worklog/worklog.db`): All runtime reads/writes happen here
- **JSONL** (`.worklog/worklog-data.jsonl`): Only exists transiently during sync operations
- **Git**: Persistent storage for collaboration

### Important Rules for Agents

1. **Work with SQLite, not JSONL**
   - Never manually edit JSONL files
   - Use the database API for all data operations
   - JSONL is only for Git transport, not for data manipulation

2. **Migration Complete**
   - The old `autoExport` feature has been removed
   - No automatic JSONL exports after database writes
   - TUI is now responsive regardless of data size

3. **Sync Behavior**
   - `wl sync` exports SQLite → JSONL → pushes to Git → deletes local JSONL
   - JSONL only exists during the sync window (seconds)
   - Working directory should not have persistent JSONL files

4. **Legacy JSONL Files**
   - If you encounter a persistent JSONL file, it may be from an older version
   - Use `wl doctor migrate` to import it into SQLite
   - Use `wl doctor migrate --delete` to import and remove the file

### For More Information

See [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) for detailed architecture documentation.
