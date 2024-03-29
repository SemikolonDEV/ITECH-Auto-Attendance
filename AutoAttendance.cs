﻿using System.Reflection;
using ITECHAutoAttendance.Extensions;
using ITECHAutoAttendance.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using SeleniumExtras.WaitHelpers;

namespace ITECHAutoAttendance;

public class AutoAttendance
{
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(12);
    private readonly Configuration _configuration;

    public AutoAttendance(Configuration configuration)
    {
        _configuration = configuration;
    }

    public void Run()
    {
        var username = _configuration.Username;
        var password = _configuration.Password;
        const string loginUrl = "https://moodle.itech-bs14.de/login/index.php";
        
        // Keep in mind, this varies depending on the current block.
        var attendanceBlockName = _configuration.AttendanceName;
        const string inscribeAttendanceKeyword = "Anwesenheit erfassen";
        
        var classUrl = _configuration.ClassCourseLink;
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
        
        var attendanceBlockLinkFailed = $"{errorMessagePrefix} '{attendanceBlockName}' link. {errorFixPrefix} the attendance name changed.";
        const string inscribeAttendanceLinkFailed = $"{errorMessagePrefix} '{inscribeAttendanceKeyword}' link. {errorFixPrefix} you have already attended today or there a no lessons currently.";
        const string presentCheckboxFailed = $"{errorMessagePrefix} '{presentAsTextLiteral}' checkbox.";
        const string submitChangesFailed = $"{errorMessagePrefix} button with id '{submitChangesHtmlId}'.";

        using var driver = GetWebDriver();
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

    private IWebDriver GetWebDriver()
    {
        var options = new ChromeOptions();
        if (_configuration.HideWindow)
        {
            options.AddArgument("--headless");
        }

        if (_configuration.UseRemoteDriver)
        {
            return new RemoteWebDriver(new Uri(_configuration.RemoteDriverUrl!), options);
        }

        return new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);
    }
}