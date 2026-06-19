using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class DragDropKeyboardExtensions
{
    public static Task DragAndDropByTestIdAsync(this IPage page, string sourceTestId, string targetTestId)
        => page.GetByTestId(sourceTestId).DragToAsync(page.GetByTestId(targetTestId));

    public static async Task DragToCoordinatesAsync(this IPage page, string selectorOrTestId, float x, float y)
    {
        var locator = selectorOrTestId.StartsWith("[") || selectorOrTestId.StartsWith(".") || selectorOrTestId.StartsWith("#")
            ? page.Locator(selectorOrTestId)
            : page.GetByTestId(selectorOrTestId);
        var box = await locator.BoundingBoxAsync() ?? throw new InvalidOperationException($"Could not find {selectorOrTestId}.");
        await page.Mouse.MoveAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(x, y);
        await page.Mouse.UpAsync();
    }

    public static async Task ResizeSplitPaneAsync(this IPage page, string splitterTestId, float deltaX = 0, float deltaY = 0)
    {
        var box = await page.GetByTestId(splitterTestId).BoundingBoxAsync() ?? throw new InvalidOperationException("Splitter not found.");
        var startX = box.X + box.Width / 2;
        var startY = box.Y + box.Height / 2;
        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(startX + deltaX, startY + deltaY);
        await page.Mouse.UpAsync();
    }

    public static Task DropFileOnAsync(this IPage page, string selector, string filePath)
        => page.Locator(selector).SetInputFilesAsync(filePath);

    public static Task SaveShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("S"));
    public static Task UndoShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("Z"));
    public static Task RedoShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("Shift+Z"));
    public static Task SelectAllShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("A"));
    public static Task CopyShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("C"));
    public static Task PasteShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("V"));
    public static Task FindShortcutAsync(this IPage page) => page.Keyboard.PressAsync(Shortcut("F"));
    public static Task FullscreenShortcutAsync(this IPage page) => page.Keyboard.PressAsync("F11");

    private static string Shortcut(string key) => OperatingSystem.IsMacOS() ? $"Meta+{key}" : $"Control+{key}";
}
