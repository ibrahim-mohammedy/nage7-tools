using System.Collections.Generic;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Linq;

namespace UIAutomation.Utils_Selenium
{
    public class CustomDriver : IWebDriver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IWebDriver _driver { get; }

        public IWebDriver GetIWebDriver() {
            return _driver;
        }

        public string Url
        {
            get
            {
                return _driver.Url;
            }

            set
            {
                _driver.Url = value;
            }
        }

        public string Title
        {
            get
            {
                return _driver.Title;
            }
        }

        public string PageSource
        {
            get
            {
                return _driver.PageSource;
            }
        }

        public string CurrentWindowHandle
        {
            get
            {
                return _driver.CurrentWindowHandle;
            }
        }

        public ReadOnlyCollection<string> WindowHandles
        {
            get
            {
                return _driver.WindowHandles;
            }
        }

        public CustomDriver(IWebDriver driver)
        {
            _driver = driver;
        }

        public WebElement FindWebElement(By @by, bool searchElement = true)
        {
            return searchElement ? new WebElement(_driver.FindElement(@by), by): new WebElement(null, @by);
        }

        public IWebElement FindElement(By @by)
        {
            return _driver.FindElement(@by);
        }

        public List<WebElement> FindWebElements(By @by)
        {
            return _driver.FindElements(@by).Select(x => new WebElement(x, @by)).ToList();
        }

        public ReadOnlyCollection<IWebElement> FindElements(By @by)
        {
            return _driver.FindElements(@by);
        }

        public void Close()
        {
            _driver.Close();
        }

        public void Quit()
        {
            _driver.Quit();
        }

        public IOptions Manage()
        {
            return _driver.Manage();
        }

        public INavigation Navigate()
        {
            return _driver.Navigate();
        }

        public ITargetLocator SwitchTo()
        {
            return _driver.SwitchTo();
        }

        public void Dispose()
        {
            _driver.Dispose();
        }
    }
}
