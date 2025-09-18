using NUnit.Framework;
using OpenQA.Selenium;
using CampusFrance.Test.DataManagement;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace CampusFrance.Test.Tests
{
    [TestFixture, Category("Students")]
    public class StudentsTests : RegistrationBase
    {
        [Test]
        public void RemplissageEtudiantInformatique()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "students.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<StudentData>(path)
                              .First(s => string.Equals(s.StudyField?.Trim(), "Informatique", StringComparison.OrdinalIgnoreCase));

            RemplirEtudiantComplet(d);
        }

        [Test]
        public void RemplissageEtudiantChimie()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "students.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<StudentData>(path)
                              .First(s => string.Equals(s.StudyField?.Trim(), "Chimie", StringComparison.OrdinalIgnoreCase));

            RemplirEtudiantComplet(d);
        }

        // --------- Méthode unique : remplit TOUT le flux Étudiant (commun + spécifique) ----------
        private void RemplirEtudiantComplet(StudentData d)
        {
            // 1) Champs communs (email, mdp, identité, pays, téléphone, nationalité, etc.)
            FillCommonFields(d);

            // 2) Fermer une éventuelle popin (avatar…)
            var closes = Driver.FindElements(By.CssSelector("button.ui-dialog-titlebar-close[title='Close']"));
            if (closes.Count > 0 && closes[0].Displayed && closes[0].Enabled)
                closes[0].Click();


            // 3) “Vous êtes : Étudiants”
            var etu = Driver.FindElement(By.Id("edit-field-publics-cibles-2"));
            if (!etu.Selected) etu.SendKeys(Keys.Space);

            // 4) Domaine d'études (Selectize)
            var domBox = Driver.FindElement(By.CssSelector("#edit-field-domaine-etudes + .selectize-control .selectize-input"));
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
            var domText = Driver.FindElement(By.CssSelector("#edit-field-domaine-etudes + .selectize-control .selectize-input .item")).Text;

            bool okEmail = emailVal == d.Email;
            bool okRadio = etu.Selected;
            bool okDom   = domText == d.StudyField;

            TestContext.WriteLine($"[Vous êtes] Étudiants sélectionné={etu.Selected}");
            TestContext.WriteLine($"[Domaine] lu='{domText}' attendu='{d.StudyField}'");

            // un objet JSON par ligne dans Data/resultatsTests.json
            var resultsPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..","..","..","Data","resultatsTests.json"));
            void Log(string check, string expected, string actual, bool passed)
            {
                var line = JsonSerializer.Serialize(new {
                    test = TestContext.CurrentContext.Test.Name,
                    category = "Students", // "Researchers" / "Institutions" selon le fichier
                    check, expected, actual, passed,
                    ts = DateTime.UtcNow.ToString("o")
                });
                File.AppendAllText(resultsPath, line + Environment.NewLine);
            }

            // 3 lignes NDJSON
            Log("Email", d.Email, emailVal, okEmail);
            Log("Vous êtes", "Étudiants", etu.Selected.ToString(), okRadio);
            Log("Domaine d'études", d.StudyField, domText, okDom);

            // Asserts
            Assert.Multiple(() =>
            {
                Assert.That(okEmail, $"[Email] attendu='{d.Email}' obtenu='{emailVal}'");
                Assert.That(okRadio, "[Vous êtes] 'Étudiants' devrait être sélectionné");
                Assert.That(okDom,   $"[Domaine] attendu='{d.StudyField}' obtenu='{domText}'");
            });


            
        }
    }
}
