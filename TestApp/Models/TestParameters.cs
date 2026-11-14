namespace TestApp.Models;

public enum TestMode
{
    All,
    Uniform,       
    Random,       
    TimedUniform,
    ByTopic
}

public class TestParameters
{
    public TestMode Mode { get; set; }
    public string? Topic { get; set; }
}