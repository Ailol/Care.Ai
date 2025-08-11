using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Care.AI.Api.Plugins;

public class UserPlugin
{
    [KernelFunction("GetUserProfile")]
    [Description("Gets the current user's profile")]
    public async Task<object> GetUserProfileAsync(
        [Description("User ID")] string userId)
    {
        await Task.Delay(100); // Simulate database query
        
        // In a real app, this would fetch from database
        return new
        {
            Id = userId,
            Name = "Current User",
            Email = $"user{userId}@example.com",
            Department = "Sales",
            LastLogin = DateTime.UtcNow.AddHours(-2)
        };
    }
    
    [KernelFunction("UpdateUserProfile")]
    [Description("Updates the current user's profile")]
    public async Task<string> UpdateUserProfileAsync(
        [Description("User ID")] string userId,
        [Description("Field to update")] string field,
        [Description("New value")] string value)
    {
        await Task.Delay(100); // Simulate update
        
        // In a real app, verify user can only update their own profile
        return $"Profile updated: {field} = {value} for user {userId}";
    }
}