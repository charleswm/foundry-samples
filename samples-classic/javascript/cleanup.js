#!/usr/bin/env node
/**
 * Centralized cleanup utility for samples-classic TypeScript/JavaScript samples
 * 
 * This script provides utilities to clean up agents, threads, vector stores, and files
 * created during sample demos in the samples-classic directory.
 * 
 * Usage:
 *   node cleanup.js --agent <agent_id>
 *   node cleanup.js --thread <thread_id>
 *   node cleanup.js --vector-store <vector_store_id>
 *   node cleanup.js --file <file_id>
 *   node cleanup.js --agent <id> --thread <id> --file <id>
 *   node cleanup.js --list-agents
 *   node cleanup.js --clean-all
 * 
 * Environment Variables:
 *   PROJECT_ENDPOINT or AZURE_AI_PROJECT_ENDPOINT: Your AI Project endpoint
 *   PROJECT_CONNECTION_STRING: Your AI Project connection string (alternative)
 */

const { AIProjectClient } = require("@azure/ai-projects");
const { DefaultAzureCredential } = require("@azure/identity");

function getProjectClient() {
  const endpoint = 
    process.env.PROJECT_ENDPOINT ||
    process.env.AZURE_AI_PROJECT_ENDPOINT ||
    process.env.AIPROJECT_CONNECTION_STRING;

  if (!endpoint) {
    console.error("❌ Error: No project endpoint found in environment variables.");
    console.error("Set one of: PROJECT_ENDPOINT, AZURE_AI_PROJECT_ENDPOINT, or AIPROJECT_CONNECTION_STRING");
    process.exit(1);
  }

  try {
    const credential = new DefaultAzureCredential();
    return AIProjectClient.fromConnectionString(endpoint, credential);
  } catch (error) {
    console.error(`❌ Error creating project client: ${error.message}`);
    process.exit(1);
  }
}

async function deleteAgent(client, agentId) {
  try {
    console.log(`🗑️  Deleting agent '${agentId}'...`);
    await client.agents.deleteAgent(agentId);
    console.log(`✅ Successfully deleted agent '${agentId}'`);
    return true;
  } catch (error) {
    if (error.statusCode === 404 || error.status === 404) {
      console.log(`⚠️  Agent '${agentId}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting agent '${agentId}': ${error.message}`);
    }
    return false;
  }
}

async function deleteThread(client, threadId) {
  try {
    console.log(`🗑️  Deleting thread '${threadId}'...`);
    await client.agents.threads.delete(threadId);
    console.log(`✅ Successfully deleted thread '${threadId}'`);
    return true;
  } catch (error) {
    if (error.statusCode === 404 || error.status === 404) {
      console.log(`⚠️  Thread '${threadId}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting thread '${threadId}': ${error.message}`);
    }
    return false;
  }
}

async function deleteVectorStore(client, vectorStoreId) {
  try {
    console.log(`🗑️  Deleting vector store '${vectorStoreId}'...`);
    await client.agents.vectorStores.delete(vectorStoreId);
    console.log(`✅ Successfully deleted vector store '${vectorStoreId}'`);
    return true;
  } catch (error) {
    if (error.statusCode === 404 || error.status === 404) {
      console.log(`⚠️  Vector store '${vectorStoreId}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting vector store '${vectorStoreId}': ${error.message}`);
    }
    return false;
  }
}

async function deleteFile(client, fileId) {
  try {
    console.log(`🗑️  Deleting file '${fileId}'...`);
    await client.agents.files.delete(fileId);
    console.log(`✅ Successfully deleted file '${fileId}'`);
    return true;
  } catch (error) {
    if (error.statusCode === 404 || error.status === 404) {
      console.log(`⚠️  File '${fileId}' not found (may have been already deleted)`);
    } else {
      console.error(`❌ Error deleting file '${fileId}': ${error.message}`);
    }
    return false;
  }
}

async function listAgents(client) {
  try {
    console.log("📋 Listing all agents...");
    const agents = [];
    for await (const agent of client.agents.list()) {
      agents.push(agent);
    }
    
    if (agents.length > 0) {
      console.log(`\nFound ${agents.length} agent(s):`);
      for (const agent of agents) {
        console.log(`  - ID: ${agent.id}, Name: ${agent.name || '(unnamed)'}, Created: ${agent.createdAt}`);
      }
    } else {
      console.log("ℹ️  No agents found");
    }
    return agents;
  } catch (error) {
    console.error(`❌ Error listing agents: ${error.message}`);
    return [];
  }
}

async function cleanAll(client) {
  console.log("⚠️  WARNING: This will attempt to delete ALL agents in your project.");
  
  // Simple synchronous prompt for Node.js
  const readline = require('readline').createInterface({
    input: process.stdin,
    output: process.stdout
  });

  return new Promise((resolve) => {
    readline.question("Are you sure you want to continue? (yes/no): ", async (answer) => {
      readline.close();
      
      if (answer.toLowerCase() !== "yes") {
        console.log("❌ Cancelled");
        resolve();
        return;
      }

      const agents = await listAgents(client);
      if (agents.length === 0) {
        resolve();
        return;
      }

      let deletedCount = 0;
      for (const agent of agents) {
        if (await deleteAgent(client, agent.id)) {
          deletedCount++;
        }
      }

      console.log(`\n✅ Cleanup complete: ${deletedCount} agent(s) deleted`);
      resolve();
    });
  });
}

async function main() {
  const args = process.argv.slice(2);
  
  const options = {
    agent: null,
    thread: null,
    vectorStore: null,
    file: null,
    listAgents: false,
    cleanAll: false
  };

  // Parse command line arguments
  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    
    if (arg === "--agent" && i + 1 < args.length) {
      options.agent = args[++i];
    } else if (arg === "--thread" && i + 1 < args.length) {
      options.thread = args[++i];
    } else if (arg === "--vector-store" && i + 1 < args.length) {
      options.vectorStore = args[++i];
    } else if (arg === "--file" && i + 1 < args.length) {
      options.file = args[++i];
    } else if (arg === "--list-agents") {
      options.listAgents = true;
    } else if (arg === "--clean-all") {
      options.cleanAll = true;
    } else if (arg === "--help" || arg === "-h") {
      console.log(`
Samples-Classic TypeScript/JavaScript - Resource Cleanup Utility

Usage:
  node cleanup.js --agent <agent_id>
  node cleanup.js --thread <thread_id>
  node cleanup.js --vector-store <vector_store_id>
  node cleanup.js --file <file_id>
  node cleanup.js --agent <id> --thread <id> --file <id>
  node cleanup.js --list-agents
  node cleanup.js --clean-all

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
      `);
      process.exit(0);
    }
  }

  // Check if any action was specified
  const hasAction = options.agent || options.thread || options.vectorStore || 
                    options.file || options.listAgents || options.cleanAll;
  
  if (!hasAction) {
    console.log("Use --help for usage information");
    process.exit(0);
  }

  console.log("🧹 Samples-Classic TypeScript/JavaScript - Resource Cleanup Utility");
  console.log("=".repeat(60));

  const client = getProjectClient();

  try {
    let cleanupPerformed = false;

    if (options.listAgents) {
      await listAgents(client);
      cleanupPerformed = true;
    }

    if (options.cleanAll) {
      await cleanAll(client);
      cleanupPerformed = true;
    }

    if (options.agent) {
      await deleteAgent(client, options.agent);
      cleanupPerformed = true;
    }

    if (options.thread) {
      await deleteThread(client, options.thread);
      cleanupPerformed = true;
    }

    if (options.vectorStore) {
      await deleteVectorStore(client, options.vectorStore);
      cleanupPerformed = true;
    }

    if (options.file) {
      await deleteFile(client, options.file);
      cleanupPerformed = true;
    }

    if (cleanupPerformed) {
      console.log("=".repeat(60));
      console.log("✅ Cleanup utility finished");
    }
  } catch (error) {
    console.error(`\n❌ Cleanup failed: ${error.message}`);
    process.exit(1);
  }
}

main();
