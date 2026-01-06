"""
Centralized cleanup utility for samples-classic Python samples.

This script provides utilities to clean up agents, threads, vector stores, and files
created during sample demos in the samples-classic directory.

Usage:
    # Clean up a specific agent
    python cleanup.py --agent <agent_id>
    
    # Clean up a thread
    python cleanup.py --thread <thread_id>
    
    # Clean up a vector store
    python cleanup.py --vector-store <vector_store_id>
    
    # Clean up a file
    python cleanup.py --file <file_id>
    
    # Clean up multiple resources
    python cleanup.py --agent <id> --thread <id> --file <id>
    
    # List all agents
    python cleanup.py --list-agents
    
    # Clean up all resources (interactive confirmation)
    python cleanup.py --clean-all

Environment Variables:
    PROJECT_ENDPOINT or AZURE_AI_PROJECT_ENDPOINT: Your AI Project endpoint
    PROJECT_CONNECTION_STRING: Your AI Project connection string (alternative)
"""

import os
import sys
import argparse
from typing import Optional, List

try:
    from azure.ai.projects import AIProjectClient
    from azure.identity import DefaultAzureCredential
    from azure.core.exceptions import ResourceNotFoundError, HttpResponseError
except ImportError:
    print("Error: Required packages not installed.")
    print("Install with: pip install azure-ai-projects azure-identity")
    sys.exit(1)


def get_project_client() -> AIProjectClient:
    """Create and return an AI Project client."""
    # Try different environment variable names
    endpoint = (
        os.getenv("PROJECT_ENDPOINT") or 
        os.getenv("AZURE_AI_PROJECT_ENDPOINT") or
        os.getenv("AIPROJECT_CONNECTION_STRING")
    )
    
    if not endpoint:
        print("❌ Error: No project endpoint found in environment variables.")
        print("Set one of: PROJECT_ENDPOINT, AZURE_AI_PROJECT_ENDPOINT, or AIPROJECT_CONNECTION_STRING")
        sys.exit(1)
    
    try:
        credential = DefaultAzureCredential()
        return AIProjectClient(endpoint=endpoint, credential=credential)
    except Exception as e:
        print(f"❌ Error creating project client: {e}")
        sys.exit(1)


def delete_agent(client: AIProjectClient, agent_id: str) -> bool:
    """Delete an agent by ID."""
    try:
        print(f"🗑️  Deleting agent '{agent_id}'...")
        client.agents.delete_agent(agent_id)
        print(f"✅ Successfully deleted agent '{agent_id}'")
        return True
    except ResourceNotFoundError:
        print(f"⚠️  Agent '{agent_id}' not found (may have been already deleted)")
        return False
    except Exception as e:
        print(f"❌ Error deleting agent '{agent_id}': {e}")
        return False


def delete_thread(client: AIProjectClient, thread_id: str) -> bool:
    """Delete a thread by ID."""
    try:
        print(f"🗑️  Deleting thread '{thread_id}'...")
        client.agents.threads.delete(thread_id)
        print(f"✅ Successfully deleted thread '{thread_id}'")
        return True
    except ResourceNotFoundError:
        print(f"⚠️  Thread '{thread_id}' not found (may have been already deleted)")
        return False
    except Exception as e:
        print(f"❌ Error deleting thread '{thread_id}': {e}")
        return False


def delete_vector_store(client: AIProjectClient, vector_store_id: str) -> bool:
    """Delete a vector store by ID."""
    try:
        print(f"🗑️  Deleting vector store '{vector_store_id}'...")
        client.agents.vector_stores.delete(vector_store_id)
        print(f"✅ Successfully deleted vector store '{vector_store_id}'")
        return True
    except ResourceNotFoundError:
        print(f"⚠️  Vector store '{vector_store_id}' not found (may have been already deleted)")
        return False
    except Exception as e:
        print(f"❌ Error deleting vector store '{vector_store_id}': {e}")
        return False


def delete_file(client: AIProjectClient, file_id: str) -> bool:
    """Delete a file by ID."""
    try:
        print(f"🗑️  Deleting file '{file_id}'...")
        client.agents.files.delete(file_id)
        print(f"✅ Successfully deleted file '{file_id}'")
        return True
    except ResourceNotFoundError:
        print(f"⚠️  File '{file_id}' not found (may have been already deleted)")
        return False
    except Exception as e:
        print(f"❌ Error deleting file '{file_id}': {e}")
        return False


def list_agents(client: AIProjectClient) -> List:
    """List all agents."""
    try:
        print("📋 Listing all agents...")
        agents = list(client.agents.list())
        if agents:
            print(f"\nFound {len(agents)} agent(s):")
            for agent in agents:
                print(f"  - ID: {agent.id}, Name: {agent.name or '(unnamed)'}, Created: {agent.created_at}")
        else:
            print("ℹ️  No agents found")
        return agents
    except Exception as e:
        print(f"❌ Error listing agents: {e}")
        return []


def clean_all(client: AIProjectClient):
    """Interactively clean up all resources."""
    print("⚠️  WARNING: This will attempt to delete ALL agents in your project.")
    response = input("Are you sure you want to continue? (yes/no): ")
    
    if response.lower() != "yes":
        print("❌ Cancelled")
        return
    
    agents = list_agents(client)
    if not agents:
        return
    
    deleted_count = 0
    for agent in agents:
        if delete_agent(client, agent.id):
            deleted_count += 1
    
    print(f"\n✅ Cleanup complete: {deleted_count} agent(s) deleted")


def main():
    parser = argparse.ArgumentParser(
        description="Clean up resources created by samples-classic Python samples",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python cleanup.py --agent asst_abc123
  python cleanup.py --agent asst_abc123 --thread thread_xyz789
  python cleanup.py --list-agents
  python cleanup.py --clean-all
        """
    )
    
    parser.add_argument("--agent", help="Agent ID to delete")
    parser.add_argument("--thread", help="Thread ID to delete")
    parser.add_argument("--vector-store", help="Vector store ID to delete")
    parser.add_argument("--file", help="File ID to delete")
    parser.add_argument("--list-agents", action="store_true", help="List all agents")
    parser.add_argument("--clean-all", action="store_true", help="Delete all agents (interactive)")
    
    args = parser.parse_args()
    
    # Check if any action was specified
    if not any([args.agent, args.thread, args.vector_store, args.file, args.list_agents, args.clean_all]):
        parser.print_help()
        sys.exit(0)
    
    print("🧹 Samples-Classic Python - Resource Cleanup Utility")
    print("=" * 60)
    
    client = get_project_client()
    
    cleanup_performed = False
    
    with client:
        if args.list_agents:
            list_agents(client)
            cleanup_performed = True
        
        if args.clean_all:
            clean_all(client)
            cleanup_performed = True
        
        if args.agent:
            delete_agent(client, args.agent)
            cleanup_performed = True
        
        if args.thread:
            delete_thread(client, args.thread)
            cleanup_performed = True
        
        if args.vector_store:
            delete_vector_store(client, args.vector_store)
            cleanup_performed = True
        
        if args.file:
            delete_file(client, args.file)
            cleanup_performed = True
    
    if cleanup_performed:
        print("=" * 60)
        print("✅ Cleanup utility finished")


if __name__ == "__main__":
    main()
