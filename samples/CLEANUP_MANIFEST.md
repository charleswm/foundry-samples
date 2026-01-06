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

## Future Enhancements

Planned improvements to cleanup scripts:
- [ ] Bulk cleanup by age/pattern
- [ ] Dry-run mode to preview deletions
- [ ] Cleanup history tracking
- [ ] Resource export before deletion
