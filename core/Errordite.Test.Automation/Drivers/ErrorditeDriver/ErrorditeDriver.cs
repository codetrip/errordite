using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Matching;
using Errordite.Test.Automation.Configuration;
using Errordite.Test.Automation.Data;
using Errordite.Test.Automation.Tests;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Raven.Client.Linq;
using System.Linq;
using Group = Errordite.Core.Domain.Organisation.Group;

namespace Errordite.Test.Automation.Drivers.ErrorditeDriver
{
    public class ErrorditeDriver
    {
        public ErrorditeDriver(AutomationSession automationSession, AutomationConfiguration configuration, IAppSession appSession)
        {
            _webDriver = new FirefoxDriver();
            _webDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5)); //wait for 5 seconds  
            _automationSession = automationSession;
            _configuration = configuration;
            _appSession = appSession;
        }

        private readonly IWebDriver _webDriver;
        private readonly AutomationConfiguration _configuration;
        private readonly IAppSession _appSession;
        private readonly AutomationSession _automationSession;

        public void Quit()
        {
            _webDriver.Quit();
        }

        private IRavenQueryable<T> Query<T>()
        {
            return _appSession.Raven.Query<T>().Customize(c => c.WaitForNonStaleResultsAsOfLastWrite());
        }

        private void Delete<T>(Expression<Func<T, bool>> predicate)
        {
            var entities = Query<T>().Where(predicate);
            foreach(var entity in entities)
                _appSession.Raven.Delete(entity);
            Query<T>().Where(predicate);
        }

        #region Common Test Setup Stuff

        public void Logout()
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/authentication/signout");
        }

        public void AdminLogIn()
        {
            Login("gaz@codetrip.co.uk", "password");
        }

        public void FlushCaches()
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/cache/flushallcaches");
        }

        public void Login()
        {
            Login(TestConstants.TestUser.Email, TestConstants.TestUser.Password);
        }
        
        public void Login(string username, string password)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/authentication/signin");
            _webDriver.FindElement(By.Id("Email")).Clear();
            _webDriver.FindElement(By.Id("Email")).SendKeys(username);
            _webDriver.FindElement(By.Id("Password")).Clear();
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.CssSelector("form[method=post] input[type=submit]")).Click();

            Assert.That(IsElementPresent(_webDriver, By.Id("dashboard")));
            LoggedInUser = Query<User>().FirstOrDefault(u => u.Email == TestConstants.TestUser.Email);
        }

        public User LoggedInUser { get; private set; }

        private void CleanUpExistingUser()
        {
            //all queries are attempts to make sure the deletes have been performed.  not entirely sure it works 100%
            Delete<Organisation>(o => o.Name == TestConstants.TestOrganisation.Name);
            Query<Organisation>().FirstOrDefault();
            var existingUser = Query<User>().FirstOrDefault(u => u.Email == TestConstants.TestUser.Email);
            if (existingUser != null)
            {
                Delete<Group>(g => g.OrganisationId == existingUser.OrganisationId);
                Delete<Application>(a => a.OrganisationId == existingUser.OrganisationId);
                Delete<Issue>(i => i.OrganisationId == existingUser.OrganisationId);
                Delete<Error>(e => e.OrganisationId == existingUser.OrganisationId);
                Delete<User>(u => u.Id == existingUser.Id);
                _appSession.Raven.SaveChanges();
                Query<Group>().First();
                Query<Application>().First();
                Query<User>().First();

                AdminLogIn();
                FlushCaches();
                Logout();
            }
        }

        public void Register(bool cleanUp = false)
        {
            CleanUpExistingUser();

            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/authentication/signup");
            _webDriver.FindElement(By.Id("FirstName")).Clear();
            _webDriver.FindElement(By.Id("FirstName")).SendKeys(TestConstants.TestUser.FirstName);
            _webDriver.FindElement(By.Id("LastName")).Clear();
            _webDriver.FindElement(By.Id("LastName")).SendKeys(TestConstants.TestUser.LastName);
            _webDriver.FindElement(By.Id("OrganisationName")).Clear();
            _webDriver.FindElement(By.Id("OrganisationName")).SendKeys(TestConstants.TestOrganisation.Name);
            _webDriver.FindElement(By.Id("Email")).Clear();
            _webDriver.FindElement(By.Id("Email")).SendKeys(TestConstants.TestUser.Email);
            _webDriver.FindElement(By.Id("Password")).Clear();
            _webDriver.FindElement(By.Id("Password")).SendKeys(TestConstants.TestUser.Password);
            _webDriver.FindElement(By.Id("ConfirmPassword")).Clear();
            _webDriver.FindElement(By.Id("ConfirmPassword")).SendKeys(TestConstants.TestUser.Password);
            _webDriver.FindElement(By.CssSelector("input[type=submit]")).Click();


            // Assert.That(IsElementPresent(WebDriver, By.Id("dashboard")));
            LoggedInUser = Query<User>().FirstOrDefault(u => u.Email == TestConstants.TestUser.Email);

            if (cleanUp)
            {
                AddDeleteActions(() => _appSession.Raven.Delete(LoggedInUser),
                                 () => Delete<Group>(u => u.Id == LoggedInUser.GroupIds.First()),
                                 () => Delete<Organisation>(o => o.Id == LoggedInUser.OrganisationId));
            }
        }

        private void AddDeleteActions(params Action[] actions)
        {
            _automationSession.DeleteActions.AddRange(actions);
        }

        public Application AddApplication(string name)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/applications/add");
            _webDriver.FindElement(By.Id("Name")).Clear();
            _webDriver.FindElement(By.Id("Name")).SendKeys(name);
            var isActiveChk = _webDriver.FindElement(By.Id("Active"));
            if (!isActiveChk.Selected)
                isActiveChk.Click();
            _webDriver.FindElement(By.CssSelector("form[method=post] input[type=submit]")).Click();
            var wait = new WebDriverWait(_webDriver, new TimeSpan(0, 0, 10));
            wait.Until(d => d.FindElement(By.CssSelector("#notifications.confirmation")));

            var orgid = LoggedInUser.OrganisationId;
            var application =
                Query<Application>().FirstOrDefault(
                    a => a.Name == name && a.OrganisationId == orgid);
            AddDeleteActions(() => Delete<Application>(a => a.Id == application.Id));
            return application;
        }

        public void DeleteApplication(string applicationId)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/applications");
            _webDriver.FindElement(By.CssSelector("a.delete-application")).Click();
            _webDriver.SwitchTo().Alert().Accept();
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/applications");
            Assert.That(!IsElementPresent(_webDriver, By.XPath("//tr[@id='{0}']".FormatWith(applicationId))));
        }

        public string CreateGroup(string name)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/groups/add");
            _webDriver.FindElement(By.Id("Name")).Clear();
            _webDriver.FindElement(By.Id("Name")).SendKeys(name);
            _webDriver.FindElement(By.CssSelector("form[method=post] input[type=submit]")).Click();

            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/groups");

            var action = _webDriver.FindElement(By.XPath("//td[text()='{0}']/../preceding-sibling::form[1]".FormatWith(name))).GetAttribute("action");
            var match = Regex.Match(action, @"^(.*)=([0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Assert.That(match.Success);

            string groupId = "groups/" + match.Groups[2].Value;
            AddDeleteActions(() => Delete<Group>(g => g.Id == groupId));
            return groupId;
        }

        public void DeleteGroup(string groupId)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl);
            _webDriver.FindElement(By.LinkText("Groups")).Click();
            _webDriver.FindElement(By.XPath("//form[@action='/groups/delete?groupId={0}']".FormatWith(GetFriendlyId(groupId)))).Submit();
            Assert.That(!IsElementPresent(_webDriver, By.XPath("//form[@action='/groups/delete?groupId={0}']".FormatWith(GetFriendlyId(groupId)))));
        }

        #endregion

        #region Navigation Helpers

        protected bool IsElementPresent(ISearchContext searchContext, By by)
        {
            try
            {
                searchContext.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected string GetAttributeValue(IWebDriver driver, By by, string attributeName)
        {
            try
            {
                var element = driver.FindElement(by);
                return element.GetAttribute(attributeName);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        protected string GetFriendlyId(string id)
        {
            if (id.Contains("/"))
            {
                return id.Split('/')[1];
            }

            return id;
        }

        private void SetValue(IWebElement element, string val)
        {
            element.Clear();
            element.SendKeys(val);
        }

        private void GoTo(string relativeUrl)
        {
            _webDriver.Navigate().GoToUrl(_configuration.ErrorditeBaseUrl + "/" + relativeUrl.TrimStart('/'));
        }

        #endregion

        #region Rules

        public void AddRule(string issueId, string prop, StringOperator op, string value)
        {
            GoTo("issue/" + new Issue(){Id = issueId}.FriendlyId);
            _webDriver.FindElement(By.CssSelector(".tabs a[data-val=rules]")).Click();
            _webDriver.FindElement(By.CssSelector("#rules a.add")).Click();
            var newRow = _webDriver.FindElement(By.CssSelector("#rules .new-rule"));
            SetRule(prop, op, value, newRow);
        }

        public void ChangeRule(string issueId, string oldProp, StringOperator? oldOp, string oldVal, string newProp, StringOperator newOp, string newVal)
        {
            GoTo("issue/" + new Issue() { Id = issueId }.FriendlyId);
            _webDriver.FindElement(By.CssSelector(".tabs a[data-val=rules]")).Click();
            var row = FindRow(oldProp, oldOp, oldVal);
            if (row == null)
                throw new AutomatedTestsException("could not find row {0} {1} {2}".FormatWith(oldProp, oldOp, oldVal));
            SetRule(newProp, newOp, newVal, row);
        }

        public void ApplyRuleUpdates(string name, string unmatchedName)
        {
            _webDriver.FindElement(By.Id("apply-rule-updates")).Click();

            SetValue(_webDriver.FindElement(By.Name("IssueNameAfterUpdate")), name);
            SetValue(_webDriver.FindElement(By.Name("UnmatchedIssueName")), unmatchedName);

            _webDriver.FindElement(By.Name("AdjustRules")).Click();

            Console.WriteLine(_webDriver.FindElement(By.CssSelector("#notifications")));
        }

        private static void SetRule(string prop, StringOperator op, string value, IWebElement row)
        {
            row.FindElement(By.CssSelector(".rule-prop option[value={0}]".FormatWith(prop))).Click();
            row.FindElement(By.CssSelector(".rule-operator option[value={0}]".FormatWith(op))).Click();
            row.FindElement(By.CssSelector(".rule-val")).Clear();
            row.FindElement(By.CssSelector(".rule-val")).SendKeys(value);
        }
        
        private IWebElement FindRow(string prop, StringOperator? op, string val)
        {
            return (from el in _webDriver.FindElements(By.CssSelector("#rules .rule"))
                    where (prop == null || IsElementPresent(el, By.CssSelector(".rule-prop option[selected][value='{0}']".FormatWith(prop))))
                          &&
                          (op == null || IsElementPresent(el, By.CssSelector(".rule-operator option[selected][value='{0}']".FormatWith(op))))
                          && 
                          (val == null || IsElementPresent(el, By.CssSelector(".rule-val[value='{0}']".FormatWith(val))))
                    select el).FirstOrDefault();
        }

        #endregion
    }
}