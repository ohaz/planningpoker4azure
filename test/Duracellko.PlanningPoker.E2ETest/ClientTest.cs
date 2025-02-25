﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Duracellko.PlanningPoker.E2ETest
{
    public class ClientTest
    {
        private static readonly string[] _availableEstimations = new string[]
        {
            "0", "\u00BD", "1", "2", "3", "5", "8", "13", "20", "40", "100", "\u221E", "?"
        };

        public ClientTest(IWebDriver browser, ServerFixture server)
        {
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public IWebDriver Browser { get; }

        public ServerFixture Server { get; }

        public IWebElement? AppElement { get; private set; }

        public IWebElement? PageContentElement { get; private set; }

        public IWebElement? PlanningPokerContainerElement { get; private set; }

        public IWebElement? ContainerElement { get; private set; }

        public IWebElement? CreateTeamForm { get; private set; }

        public IWebElement? JoinTeamForm { get; private set; }

        public IWebElement? PlanningPokerDeskElement { get; private set; }

        public IWebElement? MembersPanelElement { get; private set; }

        public Task OpenApplication()
        {
            Browser.Navigate().GoToUrl(Server.Uri);
            AppElement = Browser.FindElement(By.TagName("app"));
            Assert.IsNotNull(AppElement);
            return Task.Delay(2000);
        }

        public void AssertIndexPage()
        {
            Assert.IsNotNull(AppElement);
            PageContentElement = AppElement.FindElement(By.CssSelector("div.pageContent"));
            PlanningPokerContainerElement = PageContentElement.FindElement(By.CssSelector("div.planningPokerContainer"));
            ContainerElement = PlanningPokerContainerElement.FindElement(By.XPath("./div[@class='container']"));
            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            JoinTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='joinTeam']"));
        }

        public void FillCreateTeamForm(string team, string scrumMaster, string? deck = null, string? deckText = null)
        {
            Assert.IsNotNull(CreateTeamForm);
            var teamNameInput = CreateTeamForm.FindElement(By.Id("createTeam$teamName"));
            var scrumMasterNameInput = CreateTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));
            var selectDeckInput = CreateTeamForm.FindElement(By.Id("createTeam$selectedDeck"));

            teamNameInput.SendKeys(team);
            scrumMasterNameInput.SendKeys(scrumMaster);

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, scrumMasterNameInput.FindElements(By.XPath("../span")).Count);

            if (deck != null)
            {
                var selectDeckElement = new SelectElement(selectDeckInput);
                selectDeckElement.SelectByValue(deck);
                if (deckText != null)
                {
                    Assert.IsNotNull(selectDeckElement.SelectedOption);
                    Assert.AreEqual(deckText, selectDeckElement.SelectedOption.Text);
                }
            }
        }

        public void SubmitCreateTeamForm()
        {
            Assert.IsNotNull(CreateTeamForm);
            var submitButton = CreateTeamForm.FindElement(By.Id("createTeam$Submit"));
            submitButton.Click();
        }

        public void FillJoinTeamForm(string team, string member)
        {
            FillJoinTeamForm(team, member, false);
        }

        public void FillJoinTeamForm(string team, string member, bool asObserver)
        {
            Assert.IsNotNull(JoinTeamForm);
            var teamNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$teamName"));
            var memberNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$memberName"));
            var observerInput = JoinTeamForm.FindElement(By.Id("joinTeam$asObserver"));

            teamNameInput.SendKeys(team);
            memberNameInput.SendKeys(member);
            if (asObserver)
            {
                observerInput.Click();
            }

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, memberNameInput.FindElements(By.XPath("../span")).Count);
        }

        public void SubmitJoinTeamForm()
        {
            Assert.IsNotNull(JoinTeamForm);
            var submitButton = JoinTeamForm.FindElement(By.Id("joinTeam$submit"));
            submitButton.Click();
        }

        public void AssertPlanningPokerPage(string team, string scrumMaster)
        {
            Assert.IsNotNull(ContainerElement);
            PlanningPokerDeskElement = ContainerElement.FindElement(By.CssSelector("div.pokerDeskPanel"));
            MembersPanelElement = ContainerElement.FindElement(By.CssSelector("div.membersPanel"));

            Assert.AreEqual($"{Server.Uri}PlanningPoker/{team}/{scrumMaster}", Browser.Url);
        }

        public void AssertTeamName(string team, string member)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var teamNameHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h2"));
            Assert.AreEqual(team, teamNameHeader.Text);
            var userHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h3"));
            Assert.AreEqual(member, userHeader.Text);
        }

        public void AssertScrumMasterInTeam(string scrumMaster)
        {
            Assert.IsNotNull(MembersPanelElement);
            var scrumMasterElements = MembersPanelElement.FindElements(By.XPath("./div/ul[1]/li/span[1]"));
            Assert.AreEqual(1, scrumMasterElements.Count);
            Assert.AreEqual(scrumMaster, scrumMasterElements[0].Text);
        }

        public void AssertMembersInTeam(params string[] members)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            Assert.IsNotNull(MembersPanelElement);
            var elements = MembersPanelElement.FindElements(By.XPath("./div/ul[2]/li/span[1]"));
            Assert.AreEqual(members.Length, elements.Count);
            CollectionAssert.AreEqual(members, elements.Select(e => e.Text).ToList());
        }

        public void AssertObserversInTeam(params string[] observers)
        {
            if (observers == null)
            {
                throw new ArgumentNullException(nameof(observers));
            }

            Assert.IsNotNull(MembersPanelElement);
            var elements = MembersPanelElement.FindElements(By.XPath("./div/ul[3]/li/span"));
            Assert.AreEqual(observers.Length, elements.Count);
            CollectionAssert.AreEqual(observers, elements.Select(e => e.Text).ToList());
        }

        public void StartEstimation()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar button"));
            Assert.AreEqual("Start estimation", button.Text);

            button.Click();

            PlanningPokerDeskElement.FindElement(By.CssSelector("div.availableEstimations"));
        }

        public void CancelEstimation()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar button"));
            Assert.AreEqual("Cancel estimation", button.Text);
            button.Click();
        }

        public void ShowAverage(bool isScrumMaster)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var buttons = PlanningPokerDeskElement.FindElements(By.CssSelector("div.actionsBar button"));
            Assert.AreEqual(isScrumMaster ? 4 : 3, buttons.Count);

            var button = buttons[isScrumMaster ? 1 : 0];
            Assert.AreEqual("Show average", button.Text);
            button.Click();
        }

        public void AssertAvailableEstimations(ICollection<string>? estimations = null)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations ul li a"));
            var expectedEstimations = estimations?.ToArray() ?? _availableEstimations;
            Assert.AreEqual(expectedEstimations.Length, availableEstimationElements.Count);
            CollectionAssert.AreEqual(expectedEstimations, availableEstimationElements.Select(e => e.Text).ToList());
        }

        public void AssertNotAvailableEstimations()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations"));
            Assert.AreEqual(0, availableEstimationElements.Count);
        }

        public void SelectEstimation(string estimation)
        {
            int index = Array.IndexOf<string>(_availableEstimations, estimation);
            SelectEstimation(index);
        }

        public void SelectEstimation(int index)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations ul li a"));
            availableEstimationElements[index].Click();
        }

        public void AssertSelectedEstimation(params KeyValuePair<string, string>[] estimations)
        {
            if (estimations == null)
            {
                throw new ArgumentNullException(nameof(estimations));
            }

            Assert.IsNotNull(PlanningPokerDeskElement);
            var estimationResultElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.estimationResult ul li"));
            Assert.AreEqual(estimations.Length, estimationResultElements.Count);

            for (int i = 0; i < estimations.Length; i++)
            {
                var estimation = estimations[i];
                var estimationResultElement = estimationResultElements[i];
                var valueElement = estimationResultElement.FindElement(By.XPath("./span[1]"));
                var nameElement = estimationResultElement.FindElement(By.XPath("./span[2]"));
                Assert.AreEqual(estimation.Key, nameElement.Text);
                Assert.AreEqual(estimation.Value, valueElement.Text);
            }
        }

        public void AssertEstimationSummary(double average, double median, double sum)
        {
            var summaryItems = new[]
            {
                KeyValuePair.Create("Average", average),
                KeyValuePair.Create("Median", median),
                KeyValuePair.Create("Sum", sum),
            };

            Assert.IsNotNull(PlanningPokerDeskElement);
            var summaryItemElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.estimationResult div.estimationSummary div.estimationSummaryItem"));
            Assert.AreEqual(summaryItems.Length, summaryItemElements.Count);

            for (int i = 0; i < summaryItems.Length; i++)
            {
                var summaryItem = summaryItems[i];
                var summaryItemElement = summaryItemElements[i];
                var nameElement = summaryItemElement.FindElement(By.CssSelector("div.estimationSummaryTitle"));
                var valueElement = summaryItemElement.FindElement(By.CssSelector("div.estimationSummaryValue"));
                Assert.AreEqual(summaryItem.Key, nameElement.Text);
                Assert.AreEqual(summaryItem.Value.ToString("N2"), valueElement.Text);
            }
        }

        public void Disconnect()
        {
            Assert.IsNotNull(PlanningPokerContainerElement);
            var navbarPlanningPokerElement = PlanningPokerContainerElement.FindElement(By.TagName("nav"));
            var disconnectElement = navbarPlanningPokerElement.FindElement(By.CssSelector("ul a"));
            Assert.AreEqual("Disconnect", disconnectElement.Text);

            disconnectElement.Click();

            Assert.IsNotNull(ContainerElement);
            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            Assert.AreEqual($"{Server.Uri}Index", Browser.Url);
        }

        public void AssertMessageBox(string text)
        {
            Assert.IsNotNull(PageContentElement);
            var messageBoxElement = PageContentElement.FindElement(By.Id("messageBox"));
            Assert.AreEqual("block", messageBoxElement.GetCssValue("display"));

            var messageBodyElement = messageBoxElement.FindElement(By.CssSelector("div.modal-body"));
            Assert.AreEqual(text, messageBodyElement.Text);
        }
    }
}
