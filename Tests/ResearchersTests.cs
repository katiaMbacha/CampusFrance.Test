using NUnit.Framework;
using OpenQA.Selenium;
using CampusFrance.Test.DataManagement;
using System;
using System.IO;
using System.Linq;

namespace CampusFrance.Test.Tests
{
    [TestFixture, Category("Researchers")]
    public class ResearchersTests : RegistrationBase
    {
        [Test]
        public void RemplissageChercheurInformatique()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "researchers.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<ResearcherData>(path)
                              .First(r => string.Equals(r.StudyField?.Trim(), "Informatique", StringComparison.OrdinalIgnoreCase));

            RemplirChercheurComplet(d);
        }

        [Test]
        public void RemplissageChercheurChimie()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "researchers.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<ResearcherData>(path)
                              .First(r => string.Equals(r.StudyField?.Trim(), "Chimie", StringComparison.OrdinalIgnoreCase));

            RemplirChercheurComplet(d);
        }

        // --------- Méthode unique : communs + spécifiques Chercheur ----------
        private void RemplirChercheurComplet(ResearcherData d)
        {
            // 1) Champs communs
            FillCommonFields(d);

            // 2) Fermer éventuelle popin (avatar…)
            var closes = Driver.FindElements(By.CssSelector("button.ui-dialog-titlebar-close[title='Close']"));
            if (closes.Count > 0 && closes[0].Displayed && closes[0].Enabled)
                closes[0].Click();

            // 3) “Vous êtes : Chercheurs”
            var chercheurFor = Driver.FindElement(By.XPath("//fieldset[.//legend[contains(normalize-space(.),'Vous êtes')]]//label[normalize-space(.)='Chercheurs']")).GetAttribute("for");
            var chercheurRadio = Driver.FindElement(By.Id(chercheurFor));
            if (!chercheurRadio.Selected) chercheurRadio.SendKeys(Keys.Space);

            // 4) Domaine d'études (Selectize)
            var domBox = Wait().Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#edit-field-domaine-etudes + .selectize-control .selectize-input")));
            domBox.Click();
            Driver.SwitchTo().ActiveElement().SendKeys(Keys.Backspace);
            Driver.SwitchTo().ActiveElement().SendKeys(d.StudyField + Keys.Enter);

            // 5) Niveau(x) d'étude (Selectize)
            var levelBox = Driver.FindElement(By.CssSelector("#edit-field-niveaux-etude + .selectize-control .selectize-input"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", levelBox);
            levelBox.Click();
            Driver.SwitchTo().ActiveElement().SendKeys(Keys.Backspace);
            Driver.SwitchTo().ActiveElement().SendKeys(d.StudyLevel + Keys.Enter);

            // --- Assertions ---
            var emailVal = Driver.FindElement(By.XPath("//label[normalize-space(.)='Mon adresse e-mail']/following::input[1]")).GetAttribute("value");
            var chFor = Driver.FindElement(By.XPath("//fieldset[.//legend[contains(.,'Vous êtes')]]//label[normalize-space(.)='Chercheurs']")).GetAttribute("for")!;
            var chRadio = Driver.FindElement(By.Id(chFor));
            var levelText = Driver.FindElement(By.CssSelector("#edit-field-niveaux-etude + .selectize-control .selectize-input .item")).Text;

            bool okEmail = emailVal == d.Email;
            bool okRadio = chRadio.Selected;
            bool okLevel = levelText == d.StudyLevel;

            TestContext.WriteLine($"[Vous êtes] Chercheurs sélectionné={chRadio.Selected}");
            TestContext.WriteLine($"[Niveau] lu='{levelText}' attendu='{d.StudyLevel}'");

            // --- Log JSON via ResultWriter ---
            void Log(string check, string expected, string actual, bool passed)
            {
                ResultWriter.Append(new {
                    test = TestContext.CurrentContext.Test.Name,
                    category = "Researchers",
                    check, expected, actual, passed,
                    ts = DateTime.UtcNow.ToString("o")
                });
            }

            Log("Email", d.Email, emailVal, okEmail);
            Log("Vous êtes", "Chercheurs", chRadio.Selected.ToString(), okRadio);
            Log("Niveau d'étude", d.StudyLevel, levelText, okLevel);

            // --- Asserts ---
            Assert.Multiple(() =>
            {
                Assert.That(okEmail, $"[Email] attendu='{d.Email}' obtenu='{emailVal}'");
                Assert.That(okRadio, "[Vous êtes] 'Chercheurs' devrait être sélectionné");
                Assert.That(okLevel, $"[Niveau] attendu='{d.StudyLevel}' obtenu='{levelText}'");
            });
        }
    }
}
