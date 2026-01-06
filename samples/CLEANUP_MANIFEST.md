# Resource Cleanup Manifest

This document tracks all resource cleanup operations across the samples repository and provides guidance on using the cleanup scripts.

## Overview

Resource cleanup has been refactored into separate cleanup scripts to:
- Separate concerns between demonstration and cleanup
- Allow users to choose when to clean up resources
- Make it easier to inspect and retain resources for learning purposes
- Provide reusable cleanup utilities

## Cleanup Scripts by Sample

### Python Samples

#### 1. Code Interpreter Custom (Hosted Agents)
**Location**: `samples/python/hosted-agents/code-interpreter-custom/`

**Main Script**: `main.py`

**Cleanup Script**: `cleanup.py`

**Resources Cleaned**:
- Agent versions created during the demo

**Usage**:
```bash
# Run the main demo (creates resources)
python main.py

# Clean up resources afterwards
python cleanup.py
```

**Environment Variables Required**:
- `AZURE_AI_PROJECT_ENDPOINT`
- Agent name (passed as argument or from environment)
- Agent version (passed as argument or from environment)

---

### TypeScript Samples

#### 1. Enterprise Agent Tutorial - Idea to Prototype
**Location**: `samples/typescript/enterprise-agent-tutorial/1-idea-to-prototype/`

**Main Script**: `src/main-simple.ts`

**Cleanup Script**: `src/cleanup.ts`

**Resources Cleaned**:
- Conversations created during demos
- Agent versions created during demos

**Usage**:
```bash
# Run the main demo (creates resources)
npm start

# Clean up resources afterwards
npm run cleanup
```

**Environment Variables Required**:
- `PROJECT_ENDPOINT`
- Agent name and version (from last run or provided as arguments)
- Conversation ID (from last run or provided as arguments)

---

### C# Samples

#### 1. Enterprise Agent Tutorial - Evaluation
**Location**: `samples/csharp/enterprise-agent-tutorial/1-idea-to-prototype/Evaluate/`

**Main Script**: `Program.cs`

**Cleanup Script**: `Cleanup.cs`

**Resources Cleaned**:
- Agent versions created during evaluation
- Conversations created during evaluation

**Usage**:
```bash
# Run the evaluation (creates resources)
dotnet run

# Clean up resources afterwards
dotnet run --project Cleanup
```

**Environment Variables Required**:
- `PROJECT_ENDPOINT`
- Agent name (from evaluation run or provided as argument)

---

## Cleanup Script Conventions

All cleanup scripts follow these conventions:

1. **Idempotent**: Safe to run multiple times
2. **Informative**: Print what resources are being deleted
3. **Error Handling**: Gracefully handle missing resources
4. **Flexible Input**: Accept resource identifiers via:
   - Command-line arguments
   - Environment variables
   - Configuration files (when appropriate)
   - Interactive prompts (optional)

## Common Cleanup Operations

### Agent Versions
- **What**: Versioned agent definitions
- **Why Clean**: Prevent accumulation of test/demo versions
- **Impact**: Does not affect other versions of the same agent

### Conversations
- **What**: Chat conversation threads
- **Why Clean**: Remove demo conversation history
- **Impact**: Conversation history is lost

## Best Practices

1. **During Development**: Keep resources to inspect and debug
2. **After Learning**: Run cleanup to avoid clutter
3. **In CI/CD**: Always run cleanup in finally/cleanup stages
4. **In Production**: Use separate resource groups/projects for easy bulk cleanup

## Resource Retention Policy

Some resources you may want to keep:
- Agent versions that perform well in evaluation
- Conversations with interesting examples
- Resources in shared/production environments

## Troubleshooting

### "Resource not found" errors
- This is normal if resources were already deleted
- Cleanup scripts handle this gracefully

### Permission errors
- Ensure your credentials have delete permissions
- Check Azure RBAC roles on the AI Foundry project

### Partial cleanup
- If a script fails mid-cleanup, it's safe to run again
- Scripts are idempotent and skip already-deleted resources

---

## Samples-Classic Cleanup Utilities

### Overview

The `samples-classic` directory contains many samples using the classic agents API. Due to the high volume of samples (~59 files with cleanup code), centralized cleanup utilities have been created.

### Centralized Cleanup Scripts

#### Python Cleanup Utility
**Location**: `samples-classic/python/cleanup.py`

**Resources Cleaned**:
- Agents (classic API)
- Threads
- Vector stores
- Files

**Usage**:
```bash
# Clean up specific resources
python cleanup.py --agent asst_abc123
python cleanup.py --thread thread_xyz789
python cleanup.py --agent <id> --thread <id> --file <id>

# List all agents
python cleanup.py --list-agents

# Clean up all agents (interactive)
python cleanup.py --clean-all
```

#### TypeScript/JavaScript Cleanup Utility
**Location**: `samples-classic/javascript/cleanup.js`

**Resources Cleaned**:
- Agents (classic API)
- Threads
- Vector stores
- Files

**Usage**:
```bash
# Clean up specific resources
node cleanup.js --agent asst_abc123
node cleanup.js --thread thread_xyz789

# List all agents
node cleanup.js --list-agents

# Clean up all agents (interactive)
node cleanup.js --clean-all
```

#### C# Cleanup Utility
**Location**: `samples-classic/csharp/Cleanup/`

**Resources Cleaned**:
- Agents (classic API)
- Threads
- Vector stores
- Files

**Usage**:
```bash
# Restore dependencies first
cd samples-classic/csharp/Cleanup
dotnet restore

# Clean up specific resources
dotnet run -- --agent asst_abc123
dotnet run -- --thread thread_xyz789

# List all agents
dotnet run -- --list-agents

# Clean up all agents (interactive)
dotnet run -- --clean-all
```

### Samples Using Classic Cleanup Utilities

The centralized cleanup utilities support ~59 sample files:
- **Python**: ~28 files (quickstart, getting-started-agents, 3p-tools, agent-client)
- **C#**: ~29 files (quickstart, getting-started-agents with Async/Sync variants)
- **TypeScript**: 1 file (quickstart)
- **JavaScript**: 1 file (quickstart)

### Why Centralized for Samples-Classic?

Unlike the modern samples in `./samples`, the classic samples:
1. Use the older classic agents API (`delete_agent()` vs `delete_version()`)
2. Have many more samples (59 vs 3)
3. Often demonstrate similar patterns across multiple files
4. Would benefit from a unified cleanup approach

For detailed instructions, see `samples-classic/CLEANUP_README.md`.

---

## Future Enhancements

Planned improvements to cleanup scripts:
- [ ] Bulk cleanup by age/pattern
- [ ] Dry-run mode to preview deletions
- [ ] Cleanup history tracking
- [ ] Resource export before deletion
