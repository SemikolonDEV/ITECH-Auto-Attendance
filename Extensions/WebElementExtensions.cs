using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ITECHAutoAttendance.Extensions;

public static class WebElementExtensions
{
    public static IWebElement PerformWithTimeout(this IWebDriver driver,
        Func<IWebDriver, IWebElement> conditions,
        TimeSpan timeout,
        string errorMessage)
    {
        try
        {
            var wait = new WebDriverWait(driver, timeout);
            return wait.Until(conditions);
        }
        catch (Exception)
        {
            throw new TimeoutException(errorMessage);
        }
    }
}