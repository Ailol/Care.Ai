using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Care.AI.Api.Plugins;

public class TestPlugin
{
    [KernelFunction("TestFunction")]
    [Description("A simple test function")]
    public string TestFunction()
    {
        return "Hello from test function!";
    }
}