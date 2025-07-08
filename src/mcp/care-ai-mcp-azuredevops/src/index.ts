#!/usr/bin/env node
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from '@modelcontextprotocol/sdk/types.js';
import axios from 'axios';

const server = new Server(
  {
    name: 'care-ai-mcp-azuredevops',
    version: '1.0.0',
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

// Get organization from command line args
const organization = process.argv[2];
if (!organization) {
  console.error('Usage: care-ai-mcp-azuredevops <organization>');
  process.exit(1);
}

// Get PAT from environment variable
const pat = process.env.AZURE_DEVOPS_PAT;
if (!pat) {
  console.error('Error: AZURE_DEVOPS_PAT environment variable not set');
  process.exit(1);
}

const authHeader = `Basic ${Buffer.from(`:${pat}`).toString('base64')}`;

// List available tools
server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: 'pr_update_description',
        description: 'Update the description of a pull request',
        inputSchema: {
          type: 'object',
          properties: {
            project: {
              type: 'string',
              description: 'Project name or ID',
            },
            repositoryId: {
              type: 'string',
              description: 'Repository ID',
            },
            pullRequestId: {
              type: 'number',
              description: 'Pull request ID',
            },
            description: {
              type: 'string',
              description: 'New description for the pull request',
            },
          },
          required: ['project', 'repositoryId', 'pullRequestId', 'description'],
        },
      },
      {
        name: 'pr_get',
        description: 'Get details of a pull request',
        inputSchema: {
          type: 'object',
          properties: {
            project: {
              type: 'string',
              description: 'Project name or ID',
            },
            repositoryId: {
              type: 'string',
              description: 'Repository ID',
            },
            pullRequestId: {
              type: 'number',
              description: 'Pull request ID',
            },
          },
          required: ['project', 'repositoryId', 'pullRequestId'],
        },
      },
    ],
  };
});

// Handle tool calls
server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  switch (name) {
    case 'pr_update_description': {
      const { project, repositoryId, pullRequestId, description } = args as any;
      
      try {
        const url = `https://dev.azure.com/${organization}/${project}/_apis/git/repositories/${repositoryId}/pullrequests/${pullRequestId}?api-version=7.1`;
        
        const response = await axios.patch(
          url,
          { description },
          {
            headers: {
              'Authorization': authHeader,
              'Content-Type': 'application/json',
            },
          }
        );
        
        return {
          content: [
            {
              type: 'text',
              text: `Successfully updated PR #${pullRequestId} description`,
            },
          ],
        };
      } catch (error: any) {
        return {
          content: [
            {
              type: 'text',
              text: `Error updating PR: ${error.response?.data?.message || error.message}`,
            },
          ],
        };
      }
    }
    
    case 'pr_get': {
      const { project, repositoryId, pullRequestId } = args as any;
      
      try {
        const url = `https://dev.azure.com/${organization}/${project}/_apis/git/repositories/${repositoryId}/pullrequests/${pullRequestId}?api-version=7.1`;
        
        const response = await axios.get(url, {
          headers: {
            'Authorization': authHeader,
            'Accept': 'application/json',
          },
        });
        
        const pr = response.data;
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify({
                id: pr.pullRequestId,
                title: pr.title,
                description: pr.description,
                status: pr.status,
                sourceRefName: pr.sourceRefName,
                targetRefName: pr.targetRefName,
                createdBy: pr.createdBy.displayName,
              }, null, 2),
            },
          ],
        };
      } catch (error: any) {
        return {
          content: [
            {
              type: 'text',
              text: `Error getting PR: ${error.response?.data?.message || error.message}`,
            },
          ],
        };
      }
    }
    
    default:
      throw new Error(`Unknown tool: ${name}`);
  }
});

// Start the server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error(`Azure DevOps MCP server running for organization: ${organization}`);
}

main().catch(console.error);