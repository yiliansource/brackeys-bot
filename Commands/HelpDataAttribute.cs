using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed class HelpDataAttribute : Attribute
{
    private readonly string _usage;
    private readonly string _description;

    public string Usage { get => _usage; }
    public string Description { get => _description; }
    public string HelpMode { get; set; } = "default";
    public int ListOrder { get; set; }

    public HelpDataAttribute(string usage, string description)
    {
        _usage = usage;
        _description = description;
    }
}