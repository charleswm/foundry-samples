"""
Cleanup script for code-interpreter-custom sample.

This script deletes agent versions created during the demo to prevent
accumulation of unused resources.

Usage:
    python cleanup.py <agent_name> <agent_version>
    python cleanup.py --all  # Deletes all versions of MyAgent

Environment Variables:
    AZURE_AI_PROJECT_ENDPOINT: Your AI Project endpoint
    AGENT_NAME (optional): Default agent name to clean up
    AGENT_VERSION (optional): Specific version to clean up
"""

import os
import sys
import argparse

import dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.core.exceptions import ResourceNotFoundError

dotenv.load_dotenv()


def delete_agent_version(project_client: AIProjectClient, agent_name: str, agent_version: str) -> bool:
    """
    Delete a specific agent version.
    
    Args:
        project_client: The AI Project client
        agent_name: Name of the agent
        agent_version: Version to delete
        
    Returns:
        True if deleted successfully, False otherwise
    """
    try:
        print(f"🗑️  Deleting agent '{agent_name}' version '{agent_version}'...")
        project_client.agents.delete_version(agent_name=agent_name, agent_version=agent_version)
        print(f"✅ Successfully deleted agent '{agent_name}' version '{agent_version}'")
        return True
    except ResourceNotFoundError:
        print(f"⚠️  Agent '{agent_name}' version '{agent_version}' not found (may have been already deleted)")
        return False
    except Exception as e:
        print(f"❌ Error deleting agent '{agent_name}' version '{agent_version}': {e}")
        return False


def list_agent_versions(project_client: AIProjectClient, agent_name: str) -> list:
    """
    List all versions of an agent.
    
    Args:
        project_client: The AI Project client
        agent_name: Name of the agent
        
    Returns:
        List of agent versions
    """
    try:
        versions = list(project_client.agents.list_versions(agent_name=agent_name))
        return versions
    except ResourceNotFoundError:
        print(f"⚠️  Agent '{agent_name}' not found")
        return []
    except Exception as e:
        print(f"❌ Error listing versions for agent '{agent_name}': {e}")
        return []


def delete_all_versions(project_client: AIProjectClient, agent_name: str) -> int:
    """
    Delete all versions of an agent.
    
    Args:
        project_client: The AI Project client
        agent_name: Name of the agent
        
    Returns:
        Number of versions deleted
    """
    print(f"📋 Listing all versions of agent '{agent_name}'...")
    versions = list_agent_versions(project_client, agent_name)
    
    if not versions:
        print(f"ℹ️  No versions found for agent '{agent_name}'")
        return 0
    
    print(f"Found {len(versions)} version(s) to delete")
    deleted_count = 0
    
    for version in versions:
        if delete_agent_version(project_client, agent_name, version.version):
            deleted_count += 1
    
    return deleted_count


def main():
    parser = argparse.ArgumentParser(
        description="Clean up agent versions created by code-interpreter-custom sample"
    )
    parser.add_argument(
        "agent_name",
        nargs="?",
        default=os.getenv("AGENT_NAME", "MyAgent"),
        help="Name of the agent to clean up (default: MyAgent or AGENT_NAME env var)"
    )
    parser.add_argument(
        "agent_version",
        nargs="?",
        default=os.getenv("AGENT_VERSION"),
        help="Specific version to delete (optional, defaults to AGENT_VERSION env var)"
    )
    parser.add_argument(
        "--all",
        action="store_true",
        help="Delete all versions of the specified agent"
    )
    
    args = parser.parse_args()
    
    # Validate environment
    if not os.getenv("AZURE_AI_PROJECT_ENDPOINT"):
        print("❌ Error: AZURE_AI_PROJECT_ENDPOINT environment variable is required")
        sys.exit(1)
    
    # Create project client
    try:
        project_client = AIProjectClient(
            endpoint=os.environ["AZURE_AI_PROJECT_ENDPOINT"],
            credential=DefaultAzureCredential(),
        )
    except Exception as e:
        print(f"❌ Error creating project client: {e}")
        sys.exit(1)
    
    print("🧹 Code Interpreter Custom - Resource Cleanup")
    print("=" * 60)
    
    with project_client:
        if args.all:
            # Delete all versions
            deleted = delete_all_versions(project_client, args.agent_name)
            print("=" * 60)
            print(f"✅ Cleanup complete: {deleted} version(s) deleted")
        elif args.agent_version:
            # Delete specific version
            success = delete_agent_version(project_client, args.agent_name, args.agent_version)
            print("=" * 60)
            if success:
                print("✅ Cleanup complete")
            else:
                print("⚠️  Cleanup completed with warnings")
        else:
            print("❌ Error: Please specify an agent version or use --all flag")
            print("\nUsage examples:")
            print(f"  python cleanup.py {args.agent_name} <version>")
            print(f"  python cleanup.py {args.agent_name} --all")
            sys.exit(1)


if __name__ == "__main__":
    main()
