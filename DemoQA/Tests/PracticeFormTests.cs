using DemoQA.PageObjects;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace DemoQA.Tests
{
    [TestFixture]
    public class PracticeFormTests
    {
        private IWebDriver _driver = null!;

        [SetUp]
        public void SetUp()
        {
            var service = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1920,1080");
            _driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30));

            // сразу отключаем implicit wait
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
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
        public void FillForm_WithValidData_ShouldShowCorrectSubmission()
        {
            var page = new PracticeFormPage(_driver).Open();

            // Тестовые данные
            const string firstName = "Ivan";
            const string lastName = "Petrov";
            const string email = "ivan.petrov@example.com";
            const string gender = "Male";
            const string mobile = "1234567890";
            const string day = "15";
            const string month = "May";
            const string year = "1990";
            const string subject = "Maths";
            const string hobby = "Music";
            var outputDir = TestContext.CurrentContext.TestDirectory;
            var picturePath = Path.Combine(outputDir, "Resources", "test-image.jpg");
            const string address = "Some street 123, Riga";
            const string state = "NCR";
            const string city = "Delhi";

            page
                .SetFirstName(firstName)
                .SetLastName(lastName)
                .SetEmail(email)
                .SetGender(gender)
                .SetMobile(mobile)
                .SetDateOfBirth(day, month, year)
                .AddSubject(subject)
                .SetHobby(hobby)
                .UploadPicture(picturePath)
                .SetAddress(address)
                .SelectState(state)
                .SelectCity(city)
                .Submit();

            var result = page.GetSubmittedData();

            Assert.Multiple(() =>
            {
                Assert.That(result["Student Name"], Is.EqualTo($"{firstName} {lastName}"));
                Assert.That(result["Student Email"], Is.EqualTo(email));
                Assert.That(result["Gender"], Is.EqualTo(gender));
                Assert.That(result["Mobile"], Is.EqualTo(mobile));
                Assert.That(result["Date of Birth"], Is.EqualTo($"{day} {month},{year}"));
                Assert.That(result["Subjects"], Is.EqualTo(subject));
                Assert.That(result["Hobbies"], Is.EqualTo(hobby));
                Assert.That(result["Picture"], Is.EqualTo(Path.GetFileName(picturePath)));
                Assert.That(result["Address"], Is.EqualTo(address));
                Assert.That(result["State and City"], Is.EqualTo($"{state} {city}"));
            });

            page.CloseModal();
        }
    }
}
