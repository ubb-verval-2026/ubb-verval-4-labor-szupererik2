using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class WizzAirTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void WizzAir_ShouldHaveAtLeastTwoFlights_BucharestToBudapest_NextWeek()
    {
        // Arrange
        driver.Navigate().GoToUrl("https://wizzair.com/en-gb/flights");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        // Accept cookies if present
        try
        {
            var cookieButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-testid='cookie-policy-accept']")));
            cookieButton.Click();
        }
        catch (WebDriverTimeoutException)
        {
            // No cookie banner
        }

        // Select departure city
        var departureInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[aria-label='fc-booking-origin-aria-label']")));
        departureInput.Click();
        departureInput.SendKeys("Bucharest");
        Thread.Sleep(1000); // Wait for dropdown
        var departureOption = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[contains(text(),'Bucharest (OTP)')]")));
        departureOption.Click();

        // Select arrival city
        var arrivalInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[aria-label='fc-booking-destination-aria-label']")));
        arrivalInput.Click();
        arrivalInput.SendKeys("Budapest");
        Thread.Sleep(1000); // Wait for dropdown
        var arrivalOption = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[contains(text(),'Budapest (BUD)')]")));
        arrivalOption.Click();

        // Select dates for next week
        var today = DateTime.Now;
        var nextWeekStart = today.AddDays(7);
        var nextWeekEnd = today.AddDays(13);

        // Click on departure date
        var departureDateButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[aria-label='fc-booking-departure-date-aria-label']")));
        departureDateButton.Click();

        // Select start date
        var startDateElement = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//button[@data-date='{nextWeekStart:yyyy-MM-dd}']")));
        startDateElement.Click();

        // Select return date
        var returnDateElement = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//button[@data-date='{nextWeekEnd:yyyy-MM-dd}']")));
        returnDateElement.Click();

        // Search for flights
        var searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']")));
        searchButton.Click();

        // Wait for results
        wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".flight-list")));

        // Check for at least two flights
        var flightElements = driver.FindElements(By.CssSelector(".flight-item"));
        flightElements.Count.Should().BeGreaterOrEqualTo(2, "There should be at least two flights between Bucharest and Budapest in the next week");
    }

    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (true) // accept
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
            return alertText;
        }
        finally
        {
            // acceptNextAlert = true;
        }
    }
}