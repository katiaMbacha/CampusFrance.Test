using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;         
using System.Text.Json; 
using CampusFrance.Test.DataManagement;

namespace CampusFrance.Test.Tests
{
    public class RegistrationBase
    {
        protected IWebDriver Driver = null!;
        protected const string RegisterUrl = "https://www.campusfrance.org/fr/user/register";
        private static bool _resultsCleared = false;

        [SetUp]
        /*public void Setup()
        {

            if (!_resultsCleared)
            {
                ResultWriter.Clear("resultatsTests.json");
                _resultsCleared = true;
            }
            //Driver = new ChromeDriver();
            Driver = new FirefoxDriver();
        }*/

        public void Setup()
        {
            if (!_resultsCleared) { ResultWriter.Clear("resultatsTests.json"); _resultsCleared = true; }
        
            var headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "0") == "1";
            if (headless)
            {
                var opt = new ChromeOptions();
                opt.AddArgument("--headless=new");
                opt.AddArgument("--no-sandbox");
                opt.AddArgument("--disable-dev-shm-usage");
                opt.AddArgument("--window-size=1366,900"); // important pour éviter les overlaps
                Driver = new ChromeDriver(opt);
            }
            else
            {
                Driver = new ChromeDriver(); // tu peux remettre Firefox en local si tu veux
            }
        }

        [TearDown]
        public void TearDown()
        {
            Driver.Close();
        }

        // Méthode commune : remplit l’email depuis RegistrationBaseData
        protected void FillCommonFields(RegistrationBaseData d)
        {

        Driver.Navigate().GoToUrl(RegisterUrl);
        Thread.Sleep(3000);

        // Fermer le bandeau cookies "Tout refuser" s'il existe
        //Console.WriteLine("avant");
        // Cookies : cliquer "Tout accepter" si présent (Firefox/Chrome OK)
        ((IJavaScriptExecutor)Driver).ExecuteScript(
        "var b=document.getElementById('tarteaucitronAllAllowed')"
        + "||document.querySelector('button.tarteaucitronAllow')"
        + "||Array.from(document.querySelectorAll('button')).find(x=> (x.textContent||'').trim().includes('Tout accepter'));"
        + "if(b){ b.click(); }"
        );
        //Console.WriteLine("apres");
        


        // --- Email ---
        var emailInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Mon adresse e-mail']/following::input[1]") );
        emailInput.SendKeys(d.Email);

        // 1) Assertation Email
        var emailVal = Driver.FindElement(By.XPath("//label[normalize-space(.)='Mon adresse e-mail']/following::input[1]"))
                            .GetAttribute("value");
        TestContext.WriteLine($"[Email] lu='{emailVal}', attendu='{d.Email}'");
        Assert.That(emailVal, Is.EqualTo(d.Email), $"[Email] attendu='{d.Email}' obtenu='{emailVal}'");


        // --- Mot de passe ---
        var pwdInput = Driver.FindElement(By.Id("edit-pass-pass1"));
        pwdInput.SendKeys(d.Password);

        // Confirmation mot de passe
        var pwdConfirmInput = Driver.FindElement(By.Id("edit-pass-pass2"));
        pwdConfirmInput.SendKeys(d.PasswordConfirm);

        Thread.Sleep(3000);

        // --- Civilité (select) ---
        var civId = (d.Civility == "Mme") ? "edit-field-civilite-mme" : "edit-field-civilite-mr";
        var civRadio = Driver.FindElement(By.Id(civId));
        if (!civRadio.Selected) civRadio.SendKeys(Keys.Space);
       



        // Nom
        var lastNameInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Nom']/following::input[1]"));
        lastNameInput.SendKeys(d.LastName);
        // --- Prénom ---
        var firstNameInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Prénom']/following::input[1]"));
        firstNameInput.SendKeys(d.FirstName);

        // --- Code postal ---
        var postalInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Code postal']/following::input[1]"));
        postalInput.SendKeys(d.PostalCode);

        // --- Ville ---
        var cityInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Ville']/following::input[1]"));
        cityInput.SendKeys(d.City);

        // --- Téléphone ---
        var phoneInput = Driver.FindElement(By.XPath("//label[normalize-space(text())='Téléphone']/following::input[1]"));
        phoneInput.SendKeys(d.Phone);

        // Case "J’accepte..."

        var cb = Driver.FindElement(By.CssSelector("[data-drupal-selector='edit-field-accepte-communications-value']"));
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'center'}); arguments[0].checked = arguments[1]; arguments[0].dispatchEvent(new Event('input',{bubbles:true})); arguments[0].dispatchEvent(new Event('change',{bubbles:true}));",
            cb, d.AcceptComms
        );

        var close = Driver.FindElements(By.CssSelector("button.ui-dialog-titlebar-close[title='Close']"));
        if (close.Count > 0) close[0].Click();

        // --- Pays de résidence (Selectize) ---
        // cible la boîte visible de Selectize, pas le label
        var countryBox = Driver.FindElement(By.CssSelector("#edit-field-pays-concernes + .selectize-control .selectize-input"));
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", countryBox);
        countryBox.Click();                                // focus le champ Selectize
        Driver.SwitchTo().ActiveElement().SendKeys(Keys.Backspace);       // enlève la valeur par défaut s'il y en a une
        Driver.SwitchTo().ActiveElement().SendKeys(d.CountryResidence + Keys.Enter);  

        // --- Pays de nationalité (autocomplete) ---
        var natInput = Driver.FindElement(By.CssSelector("[data-drupal-selector='edit-field-nationalite-0-target-id']"));
        natInput.Clear();
        natInput.SendKeys(d.Nationality);                  // ex: "Maroc"
        natInput.SendKeys(Keys.ArrowDown + Keys.Enter);    // sélectionne la 1ʳᵉ suggestion

        
        //Console.WriteLine("ICI");



        }
    }
}
