using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using DemoQA.PageObjects;
using System;

namespace DemoQA.Tests
{
    [TestFixture]
    public class PracticeFormAdditionalTests
    {
        private IWebDriver _driver = null!;

        [SetUp]
        public void SetUp()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");          
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1920,1080");
            _driver = new ChromeDriver(options);
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null!;
            }
        }

        [Test]
        public void DatePicker_ShouldDisplaySelectedDateInInput()
        {
            var page = new PracticeFormPage(_driver).Open();

            // Выбираем 1 января 2000
            page.SetDateOfBirth("1", "January", "2000");

            // Проверяем value у инпута даты
            var dobInput = _driver.FindElement(By.Id("dateOfBirthInput"));
            Assert.That(dobInput.GetAttribute("value"), Is.EqualTo("01 Jan 2000"));
        }

        [Test]
        public void FillForm_WithMultipleSubjects_ShouldListAllSubjects()
        {
            var page = new PracticeFormPage(_driver).Open()
                .SetFirstName("Anna")
                .SetLastName("Ivanova")
                .SetEmail("anna.ivanova@example.com")
                .SetGender("Female")
                .SetMobile("9100000000")
                .AddSubject("Maths")
                .AddSubject("Physics")
                .Submit();

            var result = page.GetSubmittedData();
            Assert.That(result["Subjects"], Is.EqualTo("Maths, Physics"));

            page.CloseModal();
        }

        [Test]
        public void FillForm_WithMultipleHobbies_ShouldListAllHobbies()
        {
            var page = new PracticeFormPage(_driver).Open()
                .SetFirstName("Petr")
                .SetLastName("Sidorov")
                .SetEmail("petr.sidorov@example.com")
                .SetGender("Other")
                .SetMobile("9222222222")
                .SetHobby("Sports")
                .SetHobby("Reading")
                .Submit();

            var result = page.GetSubmittedData();
            Assert.That(result["Hobbies"], Is.EqualTo("Sports, Reading"));

            page.CloseModal();
        }

        [Test]
        public void Submit_WithInvalidEmail_ShouldShowValidationMessage()
        {
            var page = new PracticeFormPage(_driver).Open()
                .SetFirstName("Test")
                .SetLastName("User")
                .SetEmail("invalid-email")   // без "@"
                .SetGender("Male")
                .SetMobile("9333333333");

            // Жёсткий JS‑клик по кнопке Submit, чтобы сработала HTML5‑валидация
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].click();",
                    _driver.FindElement(By.Id("submit")));

            // Проверяем, что браузер выдал сообщение валидации
            var emailInput = _driver.FindElement(By.Id("userEmail"));
            var validationMessage = emailInput.GetAttribute("validationMessage");
            Assert.That(validationMessage, Is.Not.Empty,
                "Ожидалось, что при неверном email появится сообщение валидации.");
        }

        // Для всех комбинаций State - City
        [TestCase("NCR", "Delhi")]
        [TestCase("Uttar Pradesh", "Agra")]
        [TestCase("Uttar Pradesh", "Lucknow")]
        [TestCase("Haryana", "Karnal")]
        [TestCase("Haryana", "Panipat")]
        [TestCase("Rajasthan", "Jaipur")]
        [TestCase("Rajasthan", "Jaiselmer")]
        public void FillForm_StateAndCitySelection_ShouldDisplayCorrectStateCity(string state, string city)
        {
            // файл фотографии существует
            var picturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "test-image.jpg");
            Assert.IsTrue(File.Exists(picturePath), $"Test image not found: {picturePath}");

            var page = new PracticeFormPage(_driver)
                .Open()
                .SetFirstName("State")
                .SetLastName("City")
                .SetEmail("state.city@example.com")
                .SetGender("Other")
                .SetMobile("9876543210")
                .SetDateOfBirth("21", "August", "1992")
                .AddSubject("English")
                .SetHobby("Reading")
                .UploadPicture(picturePath)
                .SetAddress("Some address")
                .SelectState(state)
                .SelectCity(city)
                .Submit();

            var result = page.GetSubmittedData();
            Assert.That(result["State and City"], Is.EqualTo($"{state} {city}"));
            page.CloseModal();
        }

        [TestCase("29", "February", "2000")]
        [TestCase("29", "February", "2004")]
        [TestCase("28", "February", "2001")]
        [TestCase("31", "December", "1999")]
        [TestCase("01", "January", "2025")]
        public void FillForm_DateOfBirth_ShouldDisplayCorrect(string day, string month, string year)
        {
            // файл фотографии существует
            var picturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "test-image.jpg");
            Assert.IsTrue(File.Exists(picturePath), $"Test image not found: {picturePath}");

            // заполняем форму
            var page = new PracticeFormPage(_driver)
                .Open()
                .SetFirstName("Date")
                .SetLastName("Tester")
                .SetEmail("date.tester@example.com")
                .SetGender("Other")
                .SetMobile("0001112222")
                .SetDateOfBirth(day, month, year)
                .AddSubject("English")
                .SetHobby("Reading")
                .UploadPicture(picturePath)
                .SetAddress("Any address")
                .SelectState("NCR")
                .SelectCity("Delhi")
                .Submit();

            // проверяем, что дата в модалке соответствует ожиданию
            var result = page.GetSubmittedData();
            var expected = $"{day} {month},{year}";
            Assert.That(result["Date of Birth"], Is.EqualTo(expected));

            page.CloseModal();
        }
    }
}
