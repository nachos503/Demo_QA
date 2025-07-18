using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;

namespace DemoQA.PageObjects
{
    public class PracticeFormPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private const string Url = "https://demoqa.com/automation-practice-form";

        public PracticeFormPage(IWebDriver driver)
        {
            _driver = driver;
            // Отключаем implicit waits, чтобы FindElements не висел по 60 сек
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        // === Локаторы ===
        private By FirstNameInput => By.Id("firstName");
        private By LastNameInput => By.Id("lastName");
        private By EmailInput => By.Id("userEmail");
        private By GenderLabel(string g) => By.XPath($"//label[text()='{g}']");
        private By MobileInput => By.Id("userNumber");
        private By DateOfBirthInput => By.Id("dateOfBirthInput");
        private By MonthSelect => By.ClassName("react-datepicker__month-select");
        private By YearSelect => By.ClassName("react-datepicker__year-select");
        private By DayCell(string d) => By.XPath($"//div[contains(@class,'react-datepicker__day') and text()='{d}']");
        private By SubjectsInput => By.Id("subjectsInput");
        private By SubjectOption(string s) => By.XPath($"//div[contains(@id,'react-select-2-option') and text()='{s}']");
        private By HobbyCheckbox(string h) => By.XPath($"//label[text()='{h}']");
        private By UploadPictureInput => By.Id("uploadPicture");
        private By CurrentAddress => By.Id("currentAddress");
        private By StateDropdown => By.Id("react-select-3-input");
        private By CityDropdown => By.Id("react-select-4-input");
        private By SubmitButton => By.Id("submit");
        private By ModalDialog => By.ClassName("modal-content");
        private By ModalCloseButton => By.Id("closeLargeModal");
        private By ResultRows => By.XPath(".//table/tbody/tr");

        // === Методы ===
        public PracticeFormPage Open()
        {
            _driver.Navigate().GoToUrl(Url);
            _driver.Manage().Window.Maximize();
            // Закрыть плавающий баннер, если он есть
            try { _driver.FindElement(By.Id("close-fixedban")).Click(); } catch { }
            // Спрячем footer и fixedban навсегда
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                document.getElementById('fixedban')?.remove();
                document.querySelector('footer')?.remove();
            ");
            return this;
        }

        public PracticeFormPage SetFirstName(string t) { _driver.FindElement(FirstNameInput).SendKeys(t); return this; }
        public PracticeFormPage SetLastName(string t) { _driver.FindElement(LastNameInput).SendKeys(t); return this; }
        public PracticeFormPage SetEmail(string t) { _driver.FindElement(EmailInput).SendKeys(t); return this; }

        public PracticeFormPage SetGender(string g)
        {
            var label = _driver.FindElement(GenderLabel(g));
            // скролл и JS‑клик
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", label);
            return this;
        }

        public PracticeFormPage SetMobile(string t) { _driver.FindElement(MobileInput).SendKeys(t); return this; }

        public PracticeFormPage SetDateOfBirth(string d, string m, string y)
        {
            _driver.FindElement(DateOfBirthInput).Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(MonthSelect)).SendKeys(m);
            _driver.FindElement(YearSelect).SendKeys(y);
            _driver.FindElement(DayCell(d)).Click();
            return this;
        }

        public PracticeFormPage AddSubject(string subj)
        {
            var inp = _driver.FindElement(SubjectsInput);
            inp.SendKeys(subj);
            _wait.Until(ExpectedConditions.ElementToBeClickable(SubjectOption(subj))).Click();
            return this;
        }

        public PracticeFormPage SetHobby(string h)
        {
            _driver.FindElement(HobbyCheckbox(h)).Click();
            return this;
        }

        public PracticeFormPage UploadPicture(string path)
        {
            var p = path.Replace("\\", System.IO.Path.DirectorySeparatorChar.ToString());
            _driver.FindElement(UploadPictureInput).SendKeys(p);
            return this;
        }

        public PracticeFormPage SetAddress(string addr)
        {
            _driver.FindElement(CurrentAddress).SendKeys(addr);
            return this;
        }

        public PracticeFormPage SelectState(string state)
        {
            var el = _driver.FindElement(StateDropdown);
            el.SendKeys(state);
            el.SendKeys(Keys.Enter);
            return this;
        }

        public PracticeFormPage SelectCity(string city)
        {
            var el = _driver.FindElement(CityDropdown);
            el.SendKeys(city);
            el.SendKeys(Keys.Enter);
            return this;
        }

        public PracticeFormPage Submit()
        {
            _driver.FindElement(SubmitButton).Click();
            // ждём, пока появится хотя бы одна строка с результатами
            _wait.Until(d =>
            {
                var modal = d.FindElement(ModalDialog);
                return modal.FindElements(ResultRows).Count > 0;
            });
            return this;
        }

        public Dictionary<string, string> GetSubmittedData()
        {
            var modal = _driver.FindElement(ModalDialog);
            var rows = modal.FindElements(ResultRows);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                dict[cells[0].Text.Trim()] = cells[1].Text.Trim();
            }
            return dict;
        }

        public PracticeFormPage CloseModal()
        {
            // ждём, что кнопка появится и станет кликабельной
            var closeBtn = _wait.Until(d =>
            {
                var modal = d.FindElement(ModalDialog);
                var btn = modal.FindElement(ModalCloseButton);
                return (btn.Displayed && btn.Enabled) ? btn : null;
            });
            // JS‑клик
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", closeBtn);
            // ждём исчезновения модалки
            _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(ModalDialog));
            return this;
        }
    }
}
