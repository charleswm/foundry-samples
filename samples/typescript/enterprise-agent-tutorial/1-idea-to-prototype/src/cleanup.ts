#!/usr/bin/env node
/**
 * Cleanup script for enterprise-agent-tutorial sample
 * 
 * This script deletes agents and conversations created during demos
 * to prevent accumulation of unused resources.
 * 
 * Usage:
 *   npm run cleanup
 *   node dist/cleanup.js <agent_name> <agent_version>
 *   node dist/cleanup.js --conversation <conversation_id>
 *   node dist/cleanup.js --all
 * 
 * Environment Variables:
 *   PROJECT_ENDPOINT: Your Azure AI Foundry project endpoint
 *   AGENT_NAME (optional): Default agent name to clean up
 *   AGENT_VERSION (optional): Default agent version to clean up
 *   CONVERSATION_ID (optional): Conversation ID to clean up
 */

import { AgentsClient } from "@azure/ai-agents-ii";
import { getBearerTokenProvider, DefaultAzureCredential } from "@azure/identity";
import OpenAI from "openai";
import { config } from "dotenv";

config();

interface CleanupOptions {
  agentName?: string;
  agentVersion?: string;
  conversationId?: string;
  deleteAll?: boolean;
}

const projectEndpoint = process.env.PROJECT_ENDPOINT || "";

async function deleteAgentVersion(
  agentsClient: AgentsClient,
  agentName: string,
  agentVersion: string
): Promise<boolean> {
  try {
    console.log(`🗑️  Deleting agent '${agentName}' version '${agentVersion}'...`);
    await agentsClient.deleteVersion(agentName, agentVersion);
    console.log(`✅ Successfully deleted agent '${agentName}' version '${agentVersion}'`);
    return true;
  } catch (error: any) {
    if (error.statusCode === 404) {
      console.log(`⚠️  Agent '${agentName}' version '${agentVersion}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting agent '${agentName}' version '${agentVersion}': ${error.message}`);
    }
    return false;
  }
}

async function deleteConversation(
  openAIClient: OpenAI,
  conversationId: string
): Promise<boolean> {
  try {
    console.log(`🗑️  Deleting conversation '${conversationId}'...`);
    await openAIClient.conversations.delete(conversationId);
    console.log(`✅ Successfully deleted conversation '${conversationId}'`);
    return true;
  } catch (error: any) {
    if (error.status === 404) {
      console.log(`⚠️  Conversation '${conversationId}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting conversation '${conversationId}': ${error.message}`);
    }
    return false;
  }
}

async function listAgentVersions(
  agentsClient: AgentsClient,
  agentName: string
): Promise<any[]> {
  try {
    const versions = [];
    for await (const version of agentsClient.listVersions(agentName)) {
      versions.push(version);
    }
    return versions;
  } catch (error: any) {
    if (error.statusCode === 404) {
      console.log(`⚠️  Agent '${agentName}' not found`);
    } else {
      console.error(`❌ Error listing versions for agent '${agentName}': ${error.message}`);
    }
    return [];
  }
}

async function deleteAllVersions(
  agentsClient: AgentsClient,
  agentName: string
): Promise<number> {
  console.log(`📋 Listing all versions of agent '${agentName}'...`);
  const versions = await listAgentVersions(agentsClient, agentName);

  if (versions.length === 0) {
    console.log(`ℹ️  No versions found for agent '${agentName}'`);
    return 0;
  }

  console.log(`Found ${versions.length} version(s) to delete`);
  let deletedCount = 0;

  for (const version of versions) {
    if (await deleteAgentVersion(agentsClient, agentName, version.version!)) {
      deletedCount++;
    }
  }

  return deletedCount;
}

async function cleanup(options: CleanupOptions): Promise<void> {
  console.log("🧹 Enterprise Agent Tutorial - Resource Cleanup");
  console.log("=".repeat(60));

  if (!projectEndpoint) {
    console.error("❌ Error: PROJECT_ENDPOINT environment variable is required");
    process.exit(1);
  }

  try {
    const credential = new DefaultAzureCredential();

    // Create Agents client
    const agentsClient = new AgentsClient(projectEndpoint, credential, {
      apiVersion: "2025-05-15-preview",
    });

    // Create OpenAI client for conversation cleanup
    const scope = "https://ai.azure.com/.default";
    const azureADTokenProvider = await getBearerTokenProvider(credential, scope);
    const openAIClient = new OpenAI({
      apiKey: azureADTokenProvider,
      baseURL: `${projectEndpoint}/openai`,
      defaultQuery: { "api-version": "2025-05-15-preview" },
    });

    let cleanupPerformed = false;

    // Delete conversation if specified
    if (options.conversationId) {
      await deleteConversation(openAIClient, options.conversationId);
      cleanupPerformed = true;
    }

    // Delete agent version(s)
    if (options.deleteAll && options.agentName) {
      const deleted = await deleteAllVersions(agentsClient, options.agentName);
      console.log("=".repeat(60));
      console.log(`✅ Cleanup complete: ${deleted} version(s) deleted`);
      cleanupPerformed = true;
    } else if (options.agentName && options.agentVersion) {
      const success = await deleteAgentVersion(
        agentsClient,
        options.agentName,
        options.agentVersion
      );
      console.log("=".repeat(60));
      if (success) {
        console.log("✅ Cleanup complete");
      } else {
        console.log("⚠️  Cleanup completed with warnings");
      }
      cleanupPerformed = true;
    }

    if (!cleanupPerformed) {
      console.log("❌ Error: No cleanup operations specified");
      console.log("\nUsage examples:");
      console.log("  npm run cleanup -- --agent workplace-assistant --version 1");
      console.log("  npm run cleanup -- --agent workplace-assistant --all");
      console.log("  npm run cleanup -- --conversation <conversation_id>");
      console.log("  npm run cleanup -- --agent workplace-assistant --version 1 --conversation <id>");
      process.exit(1);
    }
  } catch (error: any) {
    console.error(`\n❌ Cleanup failed: ${error.message}`);
    console.error("Please check your configuration and credentials");
    process.exit(1);
  }
}

// Parse command line arguments
function parseArgs(): CleanupOptions {
  const args = process.argv.slice(2);
  const options: CleanupOptions = {
    agentName: process.env.AGENT_NAME,
    agentVersion: process.env.AGENT_VERSION,
    conversationId: process.env.CONVERSATION_ID,
  };

  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    
    if (arg === "--agent" && i + 1 < args.length) {
      options.agentName = args[++i];
    } else if (arg === "--version" && i + 1 < args.length) {
      options.agentVersion = args[++i];
    } else if (arg === "--conversation" && i + 1 < args.length) {
      options.conversationId = args[++i];
    } else if (arg === "--all") {
      options.deleteAll = true;
    } else if (!arg.startsWith("--")) {
      // Positional arguments: agent_name agent_version
      if (!options.agentName) {
        options.agentName = arg;
      } else if (!options.agentVersion) {
        options.agentVersion = arg;
      }
    }
  }

  return options;
}

// Main execution
const options = parseArgs();
cleanup(options);
