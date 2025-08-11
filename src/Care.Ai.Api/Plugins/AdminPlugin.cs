using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Care.AI.Api.Plugins;

public class AdminPlugin
{
    [KernelFunction("GetSystemInfo")]
    [Description("Gets system information - Admin only")]
    public async Task<string> GetSystemInfoAsync()
    {
        await Task.Delay(100); // Simulate work
        
        return "System Info: CPU: 85%, Memory: 4GB/8GB, Disk: 120GB/500GB";
    }
    
    [KernelFunction("GetAllUsers")]
    [Description("Gets all users in the system - Admin only")]
    public async Task<List<object>> GetAllUsersAsync()
    {
        await Task.Delay(100); // Simulate database query
        
        return new List<object>
        {
            new { Id = "1", Name = "John Admin", Role = "Administrator" },
            new { Id = "2", Name = "Jane Manager", Role = "Manager" },
            new { Id = "3", Name = "Bob User", Role = "User" }
        };
    }
    
    [KernelFunction("UpdateSystemConfig")]
    [Description("Updates system configuration - Admin only")]
    public async Task<string> UpdateSystemConfigAsync(
        [Description("Configuration key")] string key,
        [Description("Configuration value")] string value)
    {
        await Task.Delay(100); // Simulate update
        
        return $"Configuration updated: {key} = {value}";
    }
}