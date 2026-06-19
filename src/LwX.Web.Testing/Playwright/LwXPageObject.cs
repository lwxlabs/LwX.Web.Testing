using Microsoft.Playwright;

namespace LwX.Web.Testing.Playwright;

public abstract class LwXPageObject
{
    protected LwXPageObject(IPage page) => Page = page;
    protected IPage Page { get; }
    protected ILocator ByTestId(string testId) => Page.GetByTestId(testId);
}
