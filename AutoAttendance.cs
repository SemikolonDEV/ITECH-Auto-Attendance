﻿using System.Reflection;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using SeleniumExtras.WaitHelpers;

namespace ITECHAutoAttendance;

public class AutoAttendance
{
    private const string PathToAppSettings = "./appsettings.json";
    
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(12);
    private readonly Configuration _configuration;
    public string CronExpression => _configuration.CronExpression;

    public AutoAttendance()
    {
        _configuration = LoadConfiguration();
    }

    public void Start()
    {
        Console.WriteLine($"[{DateTimeOffset.Now:O}] Started trying to attendance");
        try
        {
            Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"[{DateTimeOffset.Now:O}] Failed to successfully attendance");
            return;
        }
        
        Console.WriteLine($"[{DateTimeOffset.Now:O}] Attended successfully");
    }
    
    private void Run()
    {
        string username = _configuration.Username;
        string password = _configuration.Password;
        const string loginUrl = "https://moodle.itech-bs14.de/login/index.php";
        
        // Keep in mind, this varies depending on the current block.
        string attendanceBlockName = _configuration.AttendanceBlockName;
        const string inscribeAttendanceKeyword = "Anwesenheit erfassen";
        
        const string classUrl = "https://moodle.itech-bs14.de/course/view.php?id=1570";
        const string presentAsTextLiteral = "Anwesend";
        
        const string submitChangesHtmlId = "id_submitbutton";
        const string usernameHtmlId = "username";
        const string passwordHtmlId = "password";
        const string loginButtonHtmlId = "loginbtn";

        const string errorMessagePrefix = "Failed to find";
        const string errorFixPrefix = "This may occur if";
        
        const string findUsernameFailed = $"{errorMessagePrefix} username input field.";
        const string findPasswordFailed = $"{errorMessagePrefix} password input field.";
        const string findLoginButtonFailed = $"{errorMessagePrefix} login button.";
        
        string attendanceBlockLinkFailed = $"{errorMessagePrefix} '{attendanceBlockName}' link. {errorFixPrefix} the attendance name changed.";
        const string inscribeAttendanceLinkFailed = $"{errorMessagePrefix} '{inscribeAttendanceKeyword}' link. {errorFixPrefix} you have already attended today or there a no lessons currently.";
        const string presentCheckboxFailed = $"{errorMessagePrefix} '{presentAsTextLiteral}' checkbox.";
        const string submitChangesFailed = $"{errorMessagePrefix} button with id '{submitChangesHtmlId}'.";
        
        using (var driver = GetWebDriver())
        {
            driver.Navigate().GoToUrl(loginUrl);
            
            var usernameInputElement = driver.PerformWithTimeout(ExpectedConditions.ElementIsVisible(By.Id(usernameHtmlId)), _defaultTimeout, findUsernameFailed);
            var passwordInputElement = driver.PerformWithTimeout(ExpectedConditions.ElementIsVisible(By.Id(passwordHtmlId)), _defaultTimeout, findPasswordFailed);
            usernameInputElement.SendKeys(username);
            passwordInputElement.SendKeys(password);

            var loginButtonElement = driver.PerformWithTimeout(ExpectedConditions.ElementToBeClickable(By.Id(loginButtonHtmlId)), _defaultTimeout, findLoginButtonFailed);
            loginButtonElement.Click();

            if (driver.Url == loginUrl)
            {
                throw new Exception("The browser was not redirect, inferring that the login was unsuccessful. Please double check your login credentials.");
            }
            
            driver.Navigate().GoToUrl(classUrl);

            var attendanceBlockLinkElement = driver.PerformWithTimeout(ExpectedConditions.ElementToBeClickable(By.LinkText(attendanceBlockName)), _defaultTimeout, attendanceBlockLinkFailed);
            attendanceBlockLinkElement.Click();

            var inscribeAttendanceLinkElement = driver.PerformWithTimeout(ExpectedConditions.ElementToBeClickable(By.LinkText(inscribeAttendanceKeyword)), _defaultTimeout, inscribeAttendanceLinkFailed);
            inscribeAttendanceLinkElement.Click();

            var presentCheckboxElement = driver.PerformWithTimeout(ExpectedConditions.ElementIsVisible(By.XPath($"//span[text()='{presentAsTextLiteral}']")), _defaultTimeout, presentCheckboxFailed);
            presentCheckboxElement.Click();

            var submitChangesElement = driver.PerformWithTimeout(ExpectedConditions.ElementIsVisible(By.Id(submitChangesHtmlId)), _defaultTimeout, submitChangesFailed);
            submitChangesElement.Click();
        }
    }

    private IWebDriver GetWebDriver()
    {
        if (_configuration.UseRemoteDriver)
        {
            return new RemoteWebDriver(new Uri(_configuration.RemoteDriverUrl!), new ChromeOptions());
        }

        return new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    private static Configuration LoadConfiguration()
    {
        if (File.Exists(PathToAppSettings))
        {
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(PathToAppSettings));
        }

        throw new ArgumentException("appsettings.json missing.");
    }
}