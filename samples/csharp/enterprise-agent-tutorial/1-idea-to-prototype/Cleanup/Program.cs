// Cleanup script for enterprise-agent-tutorial sample
// 
// This script deletes agents and conversations created during demos
// to prevent accumulation of unused resources.
// 
// Usage:
//   dotnet run --project Cleanup
//   dotnet run --project Cleanup -- --agent "Agent Name" --version "1"
//   dotnet run --project Cleanup -- --agent "Agent Name" --all
//   dotnet run --project Cleanup -- --conversation "conversation-id"
// 
// Environment Variables:
//   PROJECT_ENDPOINT: Your Azure AI Foundry project endpoint
//   AI_FOUNDRY_TENANT_ID (optional): Specific tenant ID
//   AGENT_NAME (optional): Default agent name to clean up
//   AGENT_VERSION (optional): Default agent version to clean up
//   CONVERSATION_ID (optional): Conversation ID to clean up

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Agents;
using Azure.Core;
using Azure.Identity;
using DotNetEnv;
using OpenAI;
using OpenAI.Responses;

class CleanupProgram
{
    static async Task Main(string[] args)
    {
        // Load environment variables from shared directory
        try
        {
            Env.Load("../shared/.env");
        }
        catch
        {
            // If shared .env doesn't exist, try local
            Env.Load(".env");
        }

        var projectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
        var tenantId = Environment.GetEnvironmentVariable("AI_FOUNDRY_TENANT_ID");

        if (string.IsNullOrEmpty(projectEndpoint))
        {
            Console.WriteLine("❌ Error: PROJECT_ENDPOINT environment variable is required");
            Environment.Exit(1);
        }

        // Parse command line arguments
        var options = ParseArgs(args);

        Console.WriteLine("🧹 Enterprise Agent Tutorial - Resource Cleanup");
        Console.WriteLine(new string('=', 60));

        try
        {
            // Use tenant-specific credential if provided
            TokenCredential credential;
            if (!string.IsNullOrEmpty(tenantId))
            {
                credential = new AzureCliCredential(new AzureCliCredentialOptions { TenantId = tenantId });
            }
            else
            {
                credential = new DefaultAzureCredential();
            }

            AgentsClient client = new(new Uri(projectEndpoint), credential);

            bool cleanupPerformed = false;

            // Delete conversation if specified
            if (!string.IsNullOrEmpty(options.ConversationId))
            {
                await DeleteConversation(client, options.ConversationId);
                cleanupPerformed = true;
            }

            // Delete agent version(s)
            if (options.DeleteAll && !string.IsNullOrEmpty(options.AgentName))
            {
                int deleted = await DeleteAllVersions(client, options.AgentName);
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"✅ Cleanup complete: {deleted} version(s) deleted");
                cleanupPerformed = true;
            }
            else if (!string.IsNullOrEmpty(options.AgentName) && !string.IsNullOrEmpty(options.AgentVersion))
            {
                bool success = await DeleteAgentVersion(client, options.AgentName, options.AgentVersion);
                Console.WriteLine(new string('=', 60));
                if (success)
                {
                    Console.WriteLine("✅ Cleanup complete");
                }
                else
                {
                    Console.WriteLine("⚠️  Cleanup completed with warnings");
                }
                cleanupPerformed = true;
            }

            if (!cleanupPerformed)
            {
                Console.WriteLine("❌ Error: No cleanup operations specified");
                Console.WriteLine("\nUsage examples:");
                Console.WriteLine("  dotnet run --project Cleanup -- --agent \"Evaluation Agent\" --version 1");
                Console.WriteLine("  dotnet run --project Cleanup -- --agent \"Evaluation Agent\" --all");
                Console.WriteLine("  dotnet run --project Cleanup -- --conversation <conversation_id>");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Cleanup failed: {ex.Message}");
            Console.WriteLine("Please check your configuration and credentials");
            Environment.Exit(1);
        }
    }

    static async Task<bool> DeleteAgentVersion(AgentsClient client, string agentName, string agentVersion)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting agent '{agentName}' version '{agentVersion}'...");
            await client.DeleteAgentVersionAsync(agentName, agentVersion);
            Console.WriteLine($"✅ Successfully deleted agent '{agentName}' version '{agentVersion}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Agent '{agentName}' version '{agentVersion}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting agent '{agentName}' version '{agentVersion}': {ex.Message}");
            return false;
        }
    }

    static async Task<bool> DeleteConversation(AgentsClient client, string conversationId)
    {
        try
        {
            Console.WriteLine($"🗑️  Deleting conversation '{conversationId}'...");
            var conversationsClient = client.GetConversationsClient();
            await conversationsClient.DeleteConversationAsync(conversationId);
            Console.WriteLine($"✅ Successfully deleted conversation '{conversationId}'");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Conversation '{conversationId}' not found (may have been already deleted)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting conversation '{conversationId}': {ex.Message}");
            return false;
        }
    }

    static async Task<List<AgentVersion>> ListAgentVersions(AgentsClient client, string agentName)
    {
        var versions = new List<AgentVersion>();
        try
        {
            await foreach (var version in client.GetAgentVersionsAsync(agentName))
            {
                versions.Add(version);
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"⚠️  Agent '{agentName}' not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing versions for agent '{agentName}': {ex.Message}");
        }
        return versions;
    }

    static async Task<int> DeleteAllVersions(AgentsClient client, string agentName)
    {
        Console.WriteLine($"📋 Listing all versions of agent '{agentName}'...");
        var versions = await ListAgentVersions(client, agentName);

        if (versions.Count == 0)
        {
            Console.WriteLine($"ℹ️  No versions found for agent '{agentName}'");
            return 0;
        }

        Console.WriteLine($"Found {versions.Count} version(s) to delete");
        int deletedCount = 0;

        foreach (var version in versions)
        {
            if (await DeleteAgentVersion(client, agentName, version.Version))
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }

    static CleanupOptions ParseArgs(string[] args)
    {
        var options = new CleanupOptions
        {
            AgentName = Environment.GetEnvironmentVariable("AGENT_NAME"),
            AgentVersion = Environment.GetEnvironmentVariable("AGENT_VERSION"),
            ConversationId = Environment.GetEnvironmentVariable("CONVERSATION_ID")
        };

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "--agent" && i + 1 < args.Length)
            {
                options.AgentName = args[++i];
            }
            else if (arg == "--version" && i + 1 < args.Length)
            {
                options.AgentVersion = args[++i];
            }
            else if (arg == "--conversation" && i + 1 < args.Length)
            {
                options.ConversationId = args[++i];
            }
            else if (arg == "--all")
            {
                options.DeleteAll = true;
            }
        }

        return options;
    }
}

class CleanupOptions
{
    public string? AgentName { get; set; }
    public string? AgentVersion { get; set; }
    public string? ConversationId { get; set; }
    public bool DeleteAll { get; set; }
}
