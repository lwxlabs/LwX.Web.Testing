using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class UiFrameworkExtensions
{
    public static Task FillAngularMaterialInputAsync(this IPage page, string label, string value) => page.GetByLabel(label).FillAsync(value);
    public static async Task SelectAngularMaterialOptionAsync(this IPage page, string label, string option)
    {
        await page.GetByLabel(label).ClickAsync();
        await page.Locator("mat-option, [role='option']").GetByText(option).ClickAsync();
    }

    public static async Task SelectVuetifyOptionAsync(this IPage page, string selectorOrTestId, string option)
    {
        var locator = selectorOrTestId.StartsWith("#") || selectorOrTestId.StartsWith(".") ? page.Locator(selectorOrTestId) : page.GetByTestId(selectorOrTestId);
        await locator.ClickAsync();
        await page.Locator(".v-list-item, [role='option']").GetByText(option).ClickAsync();
    }

    public static async Task SelectElementPlusOptionAsync(this IPage page, string selectorOrTestId, string option)
    {
        var locator = selectorOrTestId.StartsWith("#") || selectorOrTestId.StartsWith(".") ? page.Locator(selectorOrTestId) : page.GetByTestId(selectorOrTestId);
        await locator.ClickAsync();
        await page.Locator(".el-select-dropdown__item, [role='option']").GetByText(option).ClickAsync();
    }

    public static Task ExpectToastAsync(this IPage page, string text)
        => Assertions.Expect(page.Locator(".toast, .snackbar, .v-snackbar, .el-message, mat-snack-bar-container").GetByText(text)).ToBeVisibleAsync();

    public static Task ExpectDialogAsync(this IPage page)
        => Assertions.Expect(page.Locator("[role='dialog'], .modal, .v-dialog, .el-dialog, mat-dialog-container").First).ToBeVisibleAsync();
}
