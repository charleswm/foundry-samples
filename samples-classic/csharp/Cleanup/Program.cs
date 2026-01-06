// Centralized cleanup utility for samples-classic C# samples
//
// This program provides utilities to clean up agents, threads, vector stores, and files
// created during sample demos in the samples-classic directory.
//
// Usage:
//   dotnet run --project Cleanup -- --agent <agent_id>
//   dotnet run --project Cleanup -- --thread <thread_id>
//   dotnet run --project Cleanup -- --vector-store <vector_store_id>
//   dotnet run --project Cleanup -- --file <file_id>
//   dotnet run --project Cleanup -- --agent <id> --thread <id>
//   dotnet run --project Cleanup -- --list-agents
//   dotnet run --project Cleanup -- --clean-all
//
// Environment Variables:
//   PROJECT_ENDPOINT or AZURE_AI_PROJECT_ENDPOINT: Your AI Project endpoint
//   AIPROJECT_CONNECTION_STRING: Your AI Project connection string (alternative)

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;

class CleanupProgram
{
    static async Task Main(string[] args)
    {
        var options = ParseArgs(args);

        if (options.ShowHelp)
        {
            ShowHelp();
            return;
        }

        if (!HasAnyAction(options))
        {
            Console.WriteLine("Use --help for usage information");
            return;
        }

        Console.WriteLine("🧹 Samples-Classic C# - Resource Cleanup Utility");
        Console.WriteLine(new string('=', 60));

        try
        {
            var client = GetProjectClient();

            bool cleanupPerformed = false;

            if (options.ListAgents)
            {
                await ListAgents(client);
                cleanupPerformed = true;
            }

            if (options.CleanAll)
            {
                await CleanAll(client);
                cleanupPerformed = true;
            }

            if (!string.IsNullOrEmpty(options.AgentId))
            {
                await DeleteAgent(client, options.AgentId);
                cleanupPerformed = true;
            }

            if (!string.IsNullOrEmpty(options.ThreadId))
            {
                await DeleteThread(client, options.ThreadId);
                cleanupPerformed = true;
            }

            if (!string.IsNullOrEmpty(options.VectorStoreId))
            {
                await DeleteVectorStore(client, options.VectorStoreId);
                cleanupPerformed = true;
            }

            if (!string.IsNullOrEmpty(options.FileId))
            {
                await DeleteFile(client, options.FileId);
                cleanupPerformed = true;
            }

            if (cleanupPerformed)
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("✅ Cleanup utility finished");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Cleanup failed: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static AIProjectClient GetProjectClient()
    {
        var endpoint =
            Environment.GetEnvironmentVariable("PROJECT_ENDPOINT") ??
            Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ??
            Environment.GetEnvironmentVariable("AIPROJECT_CONNECTION_STRING");

        if (string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("❌ Error: No project endpoint found in environment variables.");
            Console.WriteLine("Set one of: PROJECT_ENDPOINT, AZURE_AI_PROJECT_ENDPOINT, or AIPROJECT_CONNECTION_STRING");
            Environment.Exit(1);
        }

        try
        {
            TokenCredential credential = new DefaultAzureCredential();
            return new AIProjectClient(new Uri(endpoint), credential);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating project client: {ex.Message}");
            Environment.Exit(1);
            return null!;
        }
    }

    static async Task<bool> DeleteAgent(AIProjectClient client, string agentId)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting agent '{agentId}'...");
            await client.Agents.DeleteAgentAsync(agentId);
            Console.WriteLine($"✅ Successfully deleted agent '{agentId}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Agent '{agentId}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting agent '{agentId}': {ex.Message}");
            return false;
        }
    }

    static async Task<bool> DeleteThread(AIProjectClient client, string threadId)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting thread '{threadId}'...");
            // Note: Using agents client for thread operations
            await client.Agents.Threads.DeleteThreadAsync(threadId);
            Console.WriteLine($"✅ Successfully deleted thread '{threadId}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Thread '{threadId}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting thread '{threadId}': {ex.Message}");
            return false;
        }
    }

    static async Task<bool> DeleteVectorStore(AIProjectClient client, string vectorStoreId)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting vector store '{vectorStoreId}'...");
            await client.Agents.VectorStores.DeleteVectorStoreAsync(vectorStoreId);
            Console.WriteLine($"✅ Successfully deleted vector store '{vectorStoreId}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Vector store '{vectorStoreId}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting vector store '{vectorStoreId}': {ex.Message}");
            return false;
        }
    }

    static async Task<bool> DeleteFile(AIProjectClient client, string fileId)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting file '{fileId}'...");
            await client.Agents.Files.DeleteFileAsync(fileId);
            Console.WriteLine($"✅ Successfully deleted file '{fileId}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  File '{fileId}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting file '{fileId}': {ex.Message}");
            return false;
        }
    }

    static async Task ListAgents(AIProjectClient client)
    {
        try
        {
            Console.WriteLine("📋 Listing all agents...");
            var agents = client.Agents.GetAgentsAsync().ToBlockingEnumerable().ToList();

            if (agents.Count > 0)
            {
                Console.WriteLine($"\nFound {agents.Count} agent(s):");
                foreach (var agent in agents)
                {
                    Console.WriteLine($"  - ID: {agent.Id}, Name: {agent.Name ?? "(unnamed)"}, Created: {agent.CreatedAt}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️  No agents found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing agents: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    static async Task CleanAll(AIProjectClient client)
    {
        Console.WriteLine("⚠️  WARNING: This will attempt to delete ALL agents in your project.");
        Console.Write("Are you sure you want to continue? (yes/no): ");
        var response = Console.ReadLine();

        if (response?.ToLower() != "yes")
        {
            Console.WriteLine("❌ Cancelled");
            return;
        }

        try
        {
            var agents = client.Agents.GetAgentsAsync().ToBlockingEnumerable().ToList();

            if (agents.Count == 0)
            {
                Console.WriteLine("ℹ️  No agents found");
                return;
            }

            int deletedCount = 0;
            foreach (var agent in agents)
            {
                if (await DeleteAgent(client, agent.Id))
                {
                    deletedCount++;
                }
            }

            Console.WriteLine($"\n✅ Cleanup complete: {deletedCount} agent(s) deleted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during cleanup: {ex.Message}");
        }
    }

    static CleanupOptions ParseArgs(string[] args)
    {
        var options = new CleanupOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "--agent" && i + 1 < args.Length)
            {
                options.AgentId = args[++i];
            }
            else if (arg == "--thread" && i + 1 < args.Length)
            {
                options.ThreadId = args[++i];
            }
            else if (arg == "--vector-store" && i + 1 < args.Length)
            {
                options.VectorStoreId = args[++i];
            }
            else if (arg == "--file" && i + 1 < args.Length)
            {
                options.FileId = args[++i];
            }
            else if (arg == "--list-agents")
            {
                options.ListAgents = true;
            }
            else if (arg == "--clean-all")
            {
                options.CleanAll = true;
            }
            else if (arg == "--help" || arg == "-h")
            {
                options.ShowHelp = true;
            }
        }

        return options;
    }

    static bool HasAnyAction(CleanupOptions options)
    {
        return !string.IsNullOrEmpty(options.AgentId) ||
               !string.IsNullOrEmpty(options.ThreadId) ||
               !string.IsNullOrEmpty(options.VectorStoreId) ||
               !string.IsNullOrEmpty(options.FileId) ||
               options.ListAgents ||
               options.CleanAll;
    }

    static void ShowHelp()
    {
        Console.WriteLine(@"
Samples-Classic C# - Resource Cleanup Utility

Usage:
  dotnet run --project Cleanup -- --agent <agent_id>
  dotnet run --project Cleanup -- --thread <thread_id>
  dotnet run --project Cleanup -- --vector-store <vector_store_id>
  dotnet run --project Cleanup -- --file <file_id>
  dotnet run --project Cleanup -- --agent <id> --thread <id>
  dotnet run --project Cleanup -- --list-agents
  dotnet run --project Cleanup -- --clean-all

Options:
  --agent <id>          Delete an agent by ID
  --thread <id>         Delete a thread by ID
  --vector-store <id>   Delete a vector store by ID
  --file <id>          Delete a file by ID
  --list-agents        List all agents
  --clean-all          Delete all agents (interactive confirmation)
  --help, -h           Show this help message

Environment Variables:
  PROJECT_ENDPOINT or AZURE_AI_PROJECT_ENDPOINT: Your AI Project endpoint
        ");
    }
}

class CleanupOptions
{
    public string? AgentId { get; set; }
    public string? ThreadId { get; set; }
    public string? VectorStoreId { get; set; }
    public string? FileId { get; set; }
    public bool ListAgents { get; set; }
    public bool CleanAll { get; set; }
    public bool ShowHelp { get; set; }
}
