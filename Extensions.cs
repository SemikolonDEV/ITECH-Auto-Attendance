using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ITECHAutoAttendance;

public static class Extensions
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
            Console.WriteLine(new string('-', 50));
            throw new Exception(errorMessage);
        }
    }
}