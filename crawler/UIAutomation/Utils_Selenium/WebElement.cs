using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Drawing;
using UIAutomation.Utils_Misc;
using OpenQA.Selenium.Interactions;

#pragma warning disable CS0618

namespace UIAutomation.Utils_Selenium
{
    public class WebElement : IWebElement
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IWebElement _element;

        // ENUM for possible expected conditions
        public enum ExpectedCondition { Exist, NotExists, Visible, InVisible };

        // Element attributes
        public string Value => GetAttribute("value");

        public string Checked => GetAttribute("checked");
        public string Title => GetAttribute("title");
        public string InnerHTML => GetAttribute("innerHTML");
        public string Href => GetAttribute("href");

        public IWebElement Get()
        {
            return _element;
        }

        private By BY { get; }

        public WebElement(IWebElement element, By by)
        {
            _element = element;
            BY = by;
        }

        public IWebElement FindElement(By @by)
        {
            return _element.FindElement(@by);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By @by)
        {
            return _element.FindElements(@by);
        }

        public string TagName => _element.TagName;

        public string Text => _element.Text;

        public bool Enabled => _element.Enabled;

        public bool Selected => _element.Selected;

        public Point Location => _element.Location;

        public Size Size => _element.Size;

        public bool Displayed => _element.Displayed;

        public void Clear()
        {
            _element.Clear();
        }

        public void Submit()
        {
            _element.Submit();
        }

        public string GetAttribute(string attributeName)
        {
            return _element.GetAttribute(attributeName);
        }

        public string GetProperty(string propertyName)
        {
            return _element.GetProperty(propertyName);
        }

        public string GetCssValue(string propertyName)
        {
            return _element.GetCssValue(propertyName);
        }

        #region overrideMethods

        public void Paste(string text)
        {
            System.Windows.Forms.Clipboard.SetText(text);

            _element.SendKeys(OpenQA.Selenium.Keys.Control + "v");
        }

        public void SendKeys(string text)
        {
            WaitForElementToBeVisible();
            int tries = 0;
            while (true)
            {
                try
                {
                    _element.SendKeys(text);
                    break;
                }
                catch (ElementNotInteractableException)
                {
                    if (tries == 10)
                    {
                        throw;
                    }
                    Thread.Sleep(500);
                    tries++;
                }
            }
            log.Debug($"SendKeys [{text}] to element [{BY}]");
        }

        public void Click()
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    WaitForElementToBeClickable();
                    _element.Click();
                    log.Debug($"Element clicked {BY}");
                    return;
                }
                catch (Exception e)
                {
                    retryCount++;
                    if (retryCount > 4)
                    {
                        log.Error($"Failed to click on the element {BY}. {e.Message}", e);
                        throw e;
                    }
                    Thread.Sleep(1000);
                    if (e is StaleElementReferenceException || e.InnerException is StaleElementReferenceException)
                    {
                        log.Debug("Caught stale element exception. Trying to find element again");
                        _element = DriverOperations.Get().GetIWebDriver().FindElement(BY);
                    }
                    else
                    {
                        log.Debug($"Caught exception [{e.Message}]. Retry \n{e.StackTrace}");
                    }
                }
            }
        }

        #endregion overrideMethods

        #region newMethods

        // Used to perform SendKeys and then Enter
        public void SendKeysEnter(string text)
        {
            WaitForElementToBeVisible();
            _element.SendKeys(text);
            Thread.Sleep(500);
            _element.SendKeys(Keys.Enter);
            log.Debug($"SendKeys [{text}] to element [{BY}] and hit enter");
        }

        // Upload file using send keys
        public void UploadFileUsingSendKeys(string fileName)
        {
            _element.SendKeys(Data.Get("config:fileUploadPath") + fileName);
            log.Debug($"Upload file [{fileName}] to element [{BY}]");
        }

        // Perform click using java script
        public void JsClick()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)DriverOperations.Get().GetIWebDriver();
            js.ExecuteScript("arguments[0].click();", _element);
            log.Debug($"JSclicked element: [{BY}]");
        }

        // This method waits will the given element is clickable. Typically used internally in ElementsAction.Click method after findElement
        public void WaitForElementToBeClickable(int waitTime = 0)
        {
            if (waitTime == 0)
            {
                waitTime = 20;
            }
            var wait = new WebDriverWait(DriverOperations.Get().GetIWebDriver(), TimeSpan.FromSeconds(waitTime));
            wait.Until(ExpectedConditions.ElementToBeClickable(BY));
            log.Debug($"Element {BY} is clickable now.");
        }

        // Check to see if element exists and if it does is it displayed. Optional prameter specifies wait time
        // Note: The underlying element should be called with 'searchElement'
        public void WaitForElementToDissapear(int waitTime = 0)
        {
            if (waitTime == 0)
            {
                waitTime = 20;
            }
            var elementExist = false;
            //First will check that element displayed
            for (int i = 0; i < 3; i++)
            {
                if (IsElementDisplayed())
                {
                    elementExist = true;
                    break;
                }
            }
            //Now wait until element disappeared
            if (elementExist)
            {
                for (int i = 0; i < waitTime; i++)
                {
                    Thread.Sleep(1000);
                    if (!IsElementDisplayed())
                    {
                        log.Debug($"Element {BY} disappeared after {(i + 1)} seconds");
                        return;
                    }
                }
                throw new Exception($"Element {BY} hasn't disappeared within {waitTime} seconds");
            }
            log.Debug($"Element {BY} didn't appear initially after 3 seconds.");
        }

        // Returns a boolean if an element is displayed
        // Note: The underlying element should be called with 'searchElement'
        public bool IsElementDisplayed(int timeoutSeconds = -1)
        {
            var driver = DriverOperations.Get().GetIWebDriver();
            var result = false;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            try
            {
                if (timeoutSeconds > 0)
                {
                    for (int i = 0; i < timeoutSeconds; i++)
                    {
                        List<IWebElement> list = driver.FindElements(BY).ToList();
                        if (list.Count > 0)
                        {
                            //try-catch workaround for IEdriver bug. Probably could be solved in future versions of driver
                            try
                            {
                                if (list[0].Displayed)
                                {
                                    result = true;
                                    break;
                                }
                                log.Debug($"Element  {BY} exists. but not displayed.");
                                Thread.Sleep(1000);
                            }
                            catch (Exception)
                            {
                                // Do nothing
                            }
                        }
                        i++;
                    }
                }
                else
                {
                    List<IWebElement> list = driver.FindElements(BY).ToList();
                    if (list.Count > 0)
                    {
                        //try-catch workaround for IEdriver bug. Probably could be solved in future versions of driver
                        try
                        {
                            if (list[0].Displayed)
                            {
                                result = true;
                            }
                            else
                            {
                                log.Debug($"Element  {BY} exists. but not displayed.");
                                Thread.Sleep(1000);
                                result = false;
                            }
                        }
                        catch (Exception)
                        {
                            result = false;
                            //Give one more try
                            Thread.Sleep(1000);
                            try
                            {
                                list = driver.FindElements(BY).ToList();
                                if (list.Count > 0)
                                {
                                    result = list[0].Displayed;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                DriverOperations.DefaultDriverTimeouts();
            }
            log.Debug($"Element {BY} displayed [{result.ToString().ToUpper()}]");
            return result;
        }

        // Returns a boolean true if an element exists
        // Note: The underlying element should be called with 'searchElement'
        public bool DoesElementExists(int timeoutSeconds = 1)
        {
            var driver = DriverOperations.Get().GetIWebDriver();
            var result = false;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeoutSeconds);
            try
            {
                List<IWebElement> list = driver.FindElements(BY).ToList();
                if (list.Count > 0)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                DriverOperations.DefaultDriverTimeouts();
            }
            log.Debug($"Element {BY} exists [{result.ToString().ToUpper()}]");
            return result;
        }

        // This method waits will the given element is visible. Typically used internally in ElementsAction class after findElement
        // Note: If this method is called with a wait time > defailt implcit wait time then the underlying element should be called with 'searchElement'
        public WebElement WaitForElementToBeVisible(int waitTime = 0)
        {
            var driver = DriverOperations.Get().GetIWebDriver();
            if (waitTime == 0)
            {
                waitTime = DriverOperations.implicitTimeout;
            }
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTime));
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(BY));
            }
            catch (Exception)
            {
                throw new Exception($"Element {BY} is not visible after {waitTime} seconds");
            }
            log.Debug($"Element {BY} visible");
            return this;
        }

        // This method waits will the given element is visible. Typically used internally in ElementsAction class after findElement
        public void WaitForElementToBeInvisible(int waitTime = 0)
        {
            var driver = DriverOperations.Get().GetIWebDriver();
            if (waitTime == 0)
            {
                waitTime = DriverOperations.implicitTimeout;
            }
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTime));
            try
            {
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(BY));
            }
            catch (Exception)
            {
                throw new Exception($"Element {BY} is still visible after {waitTime} seconds");
            }
            log.Debug($"Element {BY} is invisible");
        }

        // This method checks if the element text matches the given value. Optional parameter of partialmatch
        public void VerifyText(string value, bool partialmatch = false)
        {
            string str = Text;
            if (partialmatch)
            {
                if (!str.Contains(value))
                {
                    throw new Exception($"Given value [{value}] is not contained in value of the element [{str}]");
                }
            }
            else
            {
                if (str != value)
                {
                    throw new Exception($"Given value [{value}] doesn't match the value of the element [{str}]");
                }
            }
            log.Debug($"Text in Element {BY} matches value '{value}'");
        }

        //Drag&Drop one element to another
        public void DragAndDropElement(WebElement target)
        {
            Actions actions = new Actions(DriverOperations.Get().GetIWebDriver());
            actions.DragAndDrop(_element, target.Get()).Build().Perform();
            log.Info($"Element [{BY}] dragged to [{target.TagName}]");
        }

        // For RUX only. Wrapper
        public void RuxSelectDropdownMultipleTexts(string text)
        {
            RuxSelectDropdownMultipleTexts(new List<string> { text });
        }

        // For RUX only
        public void RuxSelectDropdownMultipleTexts(IEnumerable<string> options)
        {
            JsClick();
            foreach (var option in options)
            {
                DriverOperations.Get().FindWebElement(By.XPath($"//a[span[text()='{option}']]")).Click();
            }
            JsClick();
        }

        // Wrapper for a single option to select. Accepts string argument
        public void SelectDropdownMultipleTexts(string text)
        {
            SelectDropdownMultipleTexts(new List<string> { text });
        }

        // Select with multiply choices available
        public void SelectDropdownMultipleTexts(IEnumerable<string> options)
        {
            var dropdownId = _element.GetAttribute("id");
            var ulId = dropdownId.Replace("cblDiv", "cbl");
            var spanToClick = DriverOperations.Get().FindWebElement(By.XPath($"//*[@id='{dropdownId}']//span[@type='button']"));
            ScrollTo(false, spanToClick.Get());
            spanToClick.Click();
            WebElement optionsUlListElement = DriverOperations.Get().FindWebElement(By.XPath($"//ul[contains(@class,'{ulId}')]"));
            optionsUlListElement.WaitForElementToBeVisible();
            foreach (var text in options)
            {
                optionsUlListElement.ULSelectDropdownByText(text);
                if (DriverOperations.Get().FindWebElement(By.XPath($"//ul[contains(@class,'{ulId}')]/li[span[text()='{text}']][@aria-selected='true']"), false).DoesElementExists())
                {
                    continue;
                }
                // This is a retry of selecting option for such edge cases when a page has been scrolled and an incorrect option was selected at the first time.
                optionsUlListElement.WaitForElementToBeVisible();
                optionsUlListElement.ULSelectDropdownByText(text);
                DriverOperations.Get().FindWebElement(By.XPath($"//ul[contains(@class,'{ulId}')]/li[span[text()='{text}']][@aria-selected='true']"));
            }
            spanToClick.Click();
        }

        // SPECIFIC to UL elements: Select dropdown value by text
        // NOTE: Must be used on DROPDOWN DIV element. Wrapper for ULSelectDropdownByText method to automatically expand dropdown, find UL element and select option.
        public void SelectDropdownText(string text)
        {
            var dropdownId = _element.GetAttribute("id");
            ScrollTo();
            Click();
            WebElement optionsUlListElement = DriverOperations.Get().FindWebElement(By.XPath($"//div[@id='{dropdownId}']//ul[@class='chosen-results']"));
            optionsUlListElement.WaitForElementToBeVisible();
            optionsUlListElement.ULSelectDropdownByText(text);
            if (DriverOperations.Get().FindWebElement(By.XPath($"//div[@id='{dropdownId}']//ul[@class='chosen-results']/li[contains(@class,'active-result')][contains(@class,'result-selected')][contains(.,'{text}')]"), false).DoesElementExists() || DriverOperations.Get().FindWebElement(By.XPath($"//div[@id='{dropdownId}']/a[contains(.,'{text}')]")).DoesElementExists())
            {
                return;
            }
            // This is a retry of selecting option for such edge cases when a page has been scrolled and an incorrect option was selected at the first time.
            Click();
            optionsUlListElement.WaitForElementToBeVisible();
            optionsUlListElement.ULSelectDropdownByText(text);
            DriverOperations.Get().FindWebElement(By.XPath($"//div[@id='{dropdownId}']//ul[@class='chosen-results']/li[contains(@class,'active-result')][contains(@class,'result-selected')][contains(.,'{text}')]"));
        }

        // SPECIFIC to UL elements: Select dropdown value by text. This method makes 3 attempts to find the element
        [Obsolete("If possible please use 'SelectDropdownText' method which requires less steps")]
        public void ULSelectDropdownByText(string text, bool partialmatch = false)
        {
            var elementFound = false;
            int counter = 0;
            do
            {
                try
                {
                    ReadOnlyCollection<IWebElement> selectList = _element.FindElements(By.TagName("li"));
                    foreach (IWebElement li in selectList)
                    {
                        if (li.Text == text)
                        {
                            new WebElement(li, null).ScrollTo();
                            li.Click();
                            elementFound = true;
                            break;
                        }
                    }
                }
                catch (StaleElementReferenceException)
                {
                    _element = DriverOperations.Get().FindElement(BY);
                }
                counter++;
            } while (counter < 3 && !elementFound);
            if (!elementFound)
            {
                throw new Exception($"Failed to select dropdown value '{text}' in Element {BY}");
            }
            log.Debug($"Dropdown value '{text}' selected in Element  {BY}");
        }

        // SPECIFIC to select elements: Select dropdown value by text
        public void SelectDropdownByText(string text)
        {
            var selectElement = new SelectElement(_element);
            selectElement.SelectByText(text);
            log.Debug($"Dropdown value '{text}' selected in Element  {BY}");
        }

        // Uses Actions to move to an element
        public void MouseMoveTo()
        {
            Actions actions = new Actions(DriverOperations.Get().GetIWebDriver());
            actions.MoveToElement(_element).Perform();
        }

        // Uses Actions to move to an element and then click on it
        public void MouseMoveToClick()
        {
            Actions actions = new Actions(DriverOperations.Get().GetIWebDriver());
            actions.MoveToElement(_element).Click().Build().Perform();
        }

        public bool IsClickable()
        {
            try
            {
                WaitForElementToBeClickable(1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public WebElement ScrollTo(bool forceScroll = false, IWebElement element = null)
        {
            var elem = element ?? _element;
            var driver = DriverOperations.Get().GetIWebDriver();
            var elementLocation = elem.Location;
            var windowSize = driver.Manage().Window.Size;
            if (!forceScroll && !Data.Get("config:browser").ToLower().Equals("firefox"))
            {
                if (elem.Displayed)
                {
                    if (elementLocation.X <= windowSize.Width && elementLocation.Y <= windowSize.Height && elementLocation.X >= 1 && elementLocation.Y >= 1)
                    {
                        //Element visible on the view. No need to scroll.
                        return this;
                    }
                }
            }

            // Firefox. First scroll page to the top. After if element is clickable - exit method.
            if (Data.Get("config:browser").ToLower().Equals("firefox"))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");
                ((IJavaScriptExecutor)driver).ExecuteScript("window.parent.parent.scrollTo(0,0)");
                if (IsClickable() && !forceScroll)
                {
                    return this;
                }
            }

            // Perform scroll
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", elem);

            if (element == null)
            {
                log.Debug($"Scrolled view to the element [{BY}]");
            }
            else
            {
                log.Debug("Scrolled to external IWebElement");
            }
            return this;
        }

        // Clicks Elements and waits for expected condition.If expected not exist after timeout - retry click and wait
        // the underlying nxtElement should be called with 'searchElement'
        // re-try count is set to 3
        public void ClickUntilExpectedCondition(ExpectedCondition condition, WebElement nxtElement, int waitTime = 0)
        {
            int retryCounter = 0;
            bool conditionResult = false;
            if (waitTime == 0)
            {
                waitTime = DriverOperations.implicitTimeout;
            }
            do
            {
                Click();
                switch (condition)
                {
                    case ExpectedCondition.Exist:
                        conditionResult = nxtElement.DoesElementExists(waitTime);
                        break;
                    case ExpectedCondition.NotExists:
                        Thread.Sleep(waitTime * 1000); // Dirty fix. Negative scenarios will not wait till timeout
                        conditionResult = !nxtElement.DoesElementExists(waitTime);
                        break;
                    case ExpectedCondition.Visible:
                        conditionResult = nxtElement.IsElementDisplayed(waitTime);
                        break;
                    case ExpectedCondition.InVisible:
                        Thread.Sleep(waitTime * 1000); // Dirty fix. Negative scenarios will not wait till timeout
                        conditionResult = !nxtElement.IsElementDisplayed(waitTime);
                        break;
                }
            } while (!conditionResult && retryCounter++ < 3);
            if (!conditionResult)
            {
                throw new Exception($"Element [{nxtElement.BY}] has not achieved expected condition of [{condition}] after clicking element [{BY}]");
            }
            log.Debug($"Element [{BY}] clicked successfully and element [{nxtElement.BY}] has achieved expected condition of [{condition}]");
        }

        #endregion newMethods
    }
}