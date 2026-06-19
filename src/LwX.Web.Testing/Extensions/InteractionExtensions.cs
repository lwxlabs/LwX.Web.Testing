using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class InteractionExtensions
{
    public static Task ClickByTestIdAsync(this IPage page, string testId, LocatorClickOptions? options = null) => page.GetByTestId(testId).ClickAsync(options);
    public static Task DoubleClickByTestIdAsync(this IPage page, string testId) => page.GetByTestId(testId).DblClickAsync();
    public static Task RightClickByTestIdAsync(this IPage page, string testId) => page.GetByTestId(testId).ClickAsync(new() { Button = MouseButton.Right });
    public static Task HoverByTestIdAsync(this IPage page, string testId) => page.GetByTestId(testId).HoverAsync();
    public static Task FillByTestIdAsync(this IPage page, string testId, string value) => page.GetByTestId(testId).FillAsync(value);
    public static Task SelectByTestIdAsync(this IPage page, string testId, string value) => page.GetByTestId(testId).SelectOptionAsync(value);
    public static Task PressShortcutAsync(this IPage page, string shortcut) => page.Keyboard.PressAsync(shortcut);
    public static Task FillInputByLabelAsync(this IPage page, string label, string value) => page.GetByLabel(label).FillAsync(value);
    public static Task SubmitEditFormAsync(this IPage page, string selector = "form") => page.Locator(selector).PressAsync("Enter");
    public static Task ExpectValidationMessageAsync(this IPage page, string text) => Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();
    public static async Task ExpectNoValidationErrorsAsync(this IPage page)
    {
        var count = await page.Locator(".validation-message, .field-validation-error, [role='alert']").CountAsync();
        if (count > 0) throw new InvalidOperationException($"Expected no validation errors, found {count}.");
    }
}
