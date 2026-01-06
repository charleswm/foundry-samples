# Samples-Classic Cleanup Utilities

This directory contains centralized cleanup utilities for removing resources created by samples-classic demos.

## Python Cleanup

Located in `samples-classic/python/cleanup.py`

### Usage

```bash
# Clean up a specific agent
python cleanup.py --agent asst_abc123

# Clean up a thread
python cleanup.py --thread thread_xyz789

# Clean up multiple resources
python cleanup.py --agent asst_abc123 --thread thread_xyz789 --file file_abc

# List all agents
python cleanup.py --list-agents

# Clean up all agents (interactive confirmation required)
python cleanup.py --clean-all
```

### Environment Variables

Set one of the following:
- `PROJECT_ENDPOINT`
- `AZURE_AI_PROJECT_ENDPOINT`
- `AIPROJECT_CONNECTION_STRING`

## TypeScript/JavaScript Cleanup

Located in `samples-classic/javascript/cleanup.js`

### Usage

```bash
# Clean up a specific agent
node cleanup.js --agent asst_abc123

# Clean up a thread
node cleanup.js --thread thread_xyz789

# Clean up multiple resources
node cleanup.js --agent asst_abc123 --thread thread_xyz789 --file file_abc

# List all agents
node cleanup.js --list-agents

# Clean up all agents (interactive confirmation required)
node cleanup.js --clean-all
```

### Environment Variables

Set one of the following:
- `PROJECT_ENDPOINT`
- `AZURE_AI_PROJECT_ENDPOINT`
- `AIPROJECT_CONNECTION_STRING`

## C# Cleanup

Located in `samples-classic/csharp/Cleanup/`

### Usage

```bash
# First, restore dependencies
cd samples-classic/csharp/Cleanup
dotnet restore

# Clean up a specific agent
dotnet run -- --agent asst_abc123

# Clean up a thread
dotnet run -- --thread thread_xyz789

# Clean up multiple resources
dotnet run -- --agent asst_abc123 --thread thread_xyz789

# List all agents
dotnet run -- --list-agents

# Clean up all agents (interactive confirmation required)
dotnet run -- --clean-all
```

### Environment Variables

Set one of the following:
- `PROJECT_ENDPOINT`
- `AZURE_AI_PROJECT_ENDPOINT`
- `AIPROJECT_CONNECTION_STRING`

## Resource Types

These utilities can clean up:

- **Agents**: AI agents created during demos
- **Threads**: Conversation threads
- **Vector Stores**: File search vector stores
- **Files**: Uploaded files

## Best Practices

1. **During Development**: Keep resources to inspect and debug
2. **After Learning**: Run cleanup to avoid clutter
3. **In CI/CD**: Always run cleanup in finally/cleanup stages
4. **Resource IDs**: Save agent/thread IDs from demo output for cleanup

## Notes

- All cleanup operations are idempotent (safe to run multiple times)
- Missing resources are handled gracefully (warnings, not errors)
- Interactive confirmation required for `--clean-all` operations
- Cleanup utilities work with samples-classic SDK versions (classic agents API)

## Finding Resource IDs

Most samples print resource IDs when they create them. Look for output like:

```
Created agent, ID: asst_abc123
Created thread, ID: thread_xyz789
```

You can also use `--list-agents` to see all agents in your project.
