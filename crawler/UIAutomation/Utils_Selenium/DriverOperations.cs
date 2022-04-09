using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework.Internal;
using UIAutomation.Utils_Misc;

namespace UIAutomation.Utils_Selenium
{
    public class DriverOperations
    {
        // DEFAULT TIMEOUTS
        public static int pageLoadTimeout = 120;

        public static int implicitTimeout = 20;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CustomDriver Driver { get; set; }

        public static CustomDriver Get()
        {
            return (CustomDriver)TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("driver");
        }

        public static IWebDriver GetIWebDriver()
        {
            return ((CustomDriver)TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("driver"))._driver;
        }

        public DriverOperations()
        {
            instantiateDriver(Data.Get("config:browser"));
        }

        // Returns the driver. Instantiates the driver and Logins if its not a Login script
        public CustomDriver GetDriver()
        {
            if (Driver == null)
            {
                instantiateDriver(Data.Get("config:browser"));
            }
            return Driver;
        }

        // Instantiates the browser
        private void instantiateDriver(string browser)
        {
            switch (browser.ToLower())
            {
                case "firefox":
                    //                    FirefoxOptions optionsFirefox = new FirefoxOptions();
                    //                    optionsFirefox.SetPreference("moz:useNonSpecCompliantPointerOrigin", false);
                    //                    optionsFirefox.SetPreference("moz:webdriverClick", false);
                    //                    Driver = new CustomDriver(new FirefoxDriver(optionsFirefox));
                    var firefoxDriverService = FirefoxDriverService.CreateDefaultService();
                    firefoxDriverService.HideCommandPromptWindow = true;

                    FirefoxProfile ffprofile = new FirefoxProfile();
                    ffprofile.AcceptUntrustedCertificates = true;
                    ffprofile.AssumeUntrustedCertificateIssuer = true;
                    ffprofile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/csv");

                    FirefoxOptions ffOptions = new FirefoxOptions();
                    ffOptions.Profile = ffprofile;
                    ffOptions.AcceptInsecureCertificates = true;
                    ffOptions.SetPreference("dom.ipc.processCount", 1);

                    Driver = new CustomDriver(new FirefoxDriver(firefoxDriverService, ffOptions));
                    break;

                case "chrome":
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;
                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("test-type");
                    chromeOptions.AddArgument("ignore-certificate-errors");
                    chromeOptions.AddArgument("disable-infobars");
                    Driver = new CustomDriver(new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromMinutes(2)));
                    break;

                case "ie":
                    var driverService = InternetExplorerDriverService.CreateDefaultService();

                    InternetExplorerOptions options = new InternetExplorerOptions
                    {
                        IgnoreZoomLevel = true,
                    };
                    options.AddAdditionalCapability(CapabilityType.AcceptInsecureCertificates, true);
                    options.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true);
                    Driver = new CustomDriver(new InternetExplorerDriver(driverService, options, TimeSpan.FromMinutes(3)));
                    break;

                case "edge":
                    Driver = new CustomDriver(new EdgeDriver());
                    break;

                default:
                    // add exception maybe
                    break;
            }
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitTimeout);
            Driver.Manage().Window.Maximize();
        }

        // Opens up the QPA login page based on the browser and url that is passed in as parameter
        public static void OpenLoginPage(string url)
        {
            Get().Navigate().GoToUrl(url);
        }

        // Method to update the timeouts to their default values.
        public static void DefaultDriverTimeouts()
        {
            Get().Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
            Get().Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitTimeout);
        }

        // Close open browser and set driver to null
        public void CloseBrowser()
        {
            if (Driver != null)
            {
                Driver.Quit();
                Driver = null;
            }
        }

        // Switch window by title
        public static string SwitchWindowByTitle(string title)
        {
            var currentWindow = Get().CurrentWindowHandle;

            for (int retry = 0; retry < 16; retry++)
            {
                var availableWindows = new List<string>(Get().WindowHandles);

                foreach (string w in availableWindows)
                {
                    if (w == currentWindow) continue;

                    Get().SwitchTo().Window(w);

                    if (Get().Title == title) return Get().Url;
                }

                Thread.Sleep(250);
            }

            Get().SwitchTo().Window(currentWindow);

            return Get().Url;
        }

        // Switchs to the last window handel
        public static void SwitchToLastWindowHandle(int timeout = 4000)
        {
            Thread.Sleep(timeout); // Had coded wait for window to come up
            Get().SwitchTo().Window(Get().WindowHandles.LastOrDefault());
            log.Debug($"Window switched to last window handle:{Get().Title}");
        }

        // Closes the current tab and changes window handle to last current
        public static void CloseTab()
        {
            string title = Get().Title;
            Get().Close();
            log.Debug($"Window handle closed:{title}");
            SwitchToLastWindowHandle(2000);
        }
    }
}