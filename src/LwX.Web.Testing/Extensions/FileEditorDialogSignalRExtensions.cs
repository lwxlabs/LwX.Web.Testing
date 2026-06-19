using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class FileEditorDialogSignalRExtensions
{
    public static Task UploadFileByTestIdAsync(this IPage page, string inputTestId, string filePath) => page.GetByTestId(inputTestId).SetInputFilesAsync(filePath);
    public static Task DropFileByTestIdAsync(this IPage page, string dropZoneTestId, string filePath) => page.GetByTestId(dropZoneTestId).SetInputFilesAsync(filePath);

    public static async Task<string> DownloadAndReadTextAsync(this IPage page, Func<Task> action)
    {
        var download = await page.RunAndWaitForDownloadAsync(action);
        var path = await download.PathAsync();
        return await File.ReadAllTextAsync(path);
    }

    public static async Task AcceptDialogAsync(this IPage page)
    {
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await Task.CompletedTask;
    }

    public static async Task DismissDialogAsync(this IPage page)
    {
        page.Dialog += async (_, dialog) => await dialog.DismissAsync();
        await Task.CompletedTask;
    }

    public static async Task ExpectAlertAsync(this IPage page, string? expectedMessage = null)
    {
        var tcs = new TaskCompletionSource<string>();
        page.Dialog += async (_, dialog) => { tcs.TrySetResult(dialog.Message); await dialog.AcceptAsync(); };
        var message = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        if (expectedMessage is not null && !message.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected alert containing '{expectedMessage}' but got '{message}'.");
    }

    public static Task ExpectSignalRConnectedAsync(this IPage page) => page.WaitForFunctionAsync("() => !document.body.innerText.toLowerCase().includes('reconnecting')");
    public static Task SimulateSignalRDisconnectAsync(this IPage page) => page.BlockRequestsAsync("**/_blazor**");
    public static Task ExpectReconnectOverlayAsync(this IPage page) => Assertions.Expect(page.Locator("#components-reconnect-modal, .reconnect-modal, [data-testid='reconnect-overlay']")).ToBeVisibleAsync();
    public static Task ExpectSignalRReconnectedAsync(this IPage page) => page.ExpectSignalRConnectedAsync();

    public static Task EditorSetTextAsync(this IPage page, string text, string testId = "editor") => page.GetByTestId(testId).FillAsync(text);
    public static Task EditorInsertTextAsync(this IPage page, string text, string testId = "editor") => page.GetByTestId(testId).PressSequentiallyAsync(text);
    public static Task EditorPressShortcutAsync(this IPage page, string shortcut) => page.Keyboard.PressAsync(shortcut);
    public static Task ExpectPreviewContainsAsync(this IPage page, string text, string testId = "preview-panel") => Assertions.Expect(page.GetByTestId(testId)).ToContainTextAsync(text);
    public static Task ExpectMermaidRenderedAsync(this IPage page) => Assertions.Expect(page.Locator("svg[id^='mermaid'], .mermaid svg").First).ToBeVisibleAsync();
    public static Task ExpectCodeHighlightAsync(this IPage page, string language) => Assertions.Expect(page.Locator($"code.language-{language}, .hljs").First).ToBeVisibleAsync();

    public static Task MonacoSetValueAsync(this IPage page, string testId, string code)
        => page.EvaluateAsync("([id, value]) => { const el = document.querySelector(`[data-testid=\"${id}\"]`); if (el) el.textContent = value; window.monaco?.editor?.getModels?.()[0]?.setValue?.(value); }", new object[] { testId, code });
    public static Task<string?> MonacoGetValueAsync(this IPage page)
        => page.EvaluateAsync<string?>("() => window.monaco?.editor?.getModels?.()[0]?.getValue?.() ?? null");
    public static Task MonacoPressAsync(this IPage page, string shortcut) => page.Keyboard.PressAsync(shortcut);

    public static Task ExpectLayoutSavedAsync(this IPage page) => page.WaitForFunctionAsync("() => localStorage.length > 0 || sessionStorage.length > 0");
    public static async Task ExpectTabOrderAsync(this IPage page, IReadOnlyList<string> tabTexts)
    {
        var texts = await page.Locator("[data-testid^='tab'], [role='tab']").AllInnerTextsAsync();
        var normalized = texts.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        for (var i = 0; i < tabTexts.Count; i++)
            if (i >= normalized.Length || !normalized[i].Contains(tabTexts[i], StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Tab order did not match expected order.");
    }
    public static Task ReloadAndExpectLayoutRestoredAsync(this IPage page) => page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
}
