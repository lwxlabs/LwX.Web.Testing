namespace LwX.Web.Testing.Selectors;

public static class TestIds
{
    public const string SaveButton = "save-button";
    public const string CancelButton = "cancel-button";
    public const string DeleteButton = "delete-button";
    public const string SearchInput = "search-input";
    public const string Editor = "editor";
    public const string PreviewPanel = "preview-panel";
    public const string FileTree = "file-tree";
    public const string Splitter = "splitter";
    public const string Tab = "tab";
}

public static class WebTestId
{
    public static string Button(string name) => $"button-{name}";
    public static string Input(string name) => $"input-{name}";
    public static string Tab(string name) => $"tab-{name}";
    public static string Panel(string name) => $"panel-{name}";
    public static string Row(string name) => $"row-{name}";
}
