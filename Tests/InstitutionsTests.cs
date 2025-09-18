using NUnit.Framework;
using OpenQA.Selenium;
using CampusFrance.Test.DataManagement;
using System;
using System.IO;
using System.Linq;

namespace CampusFrance.Test.Tests
{
    [TestFixture, Category("Institutions")]
    public class InstitutionsTests : RegistrationBase
    {
        [Test]
        public void RemplissageInstitutionUniversite()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "institutions.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<InstitutionData>(path)
                              .First(x => string.Equals(x.OrganizationType?.Trim(), "Etablissement-Université-Ecole", StringComparison.OrdinalIgnoreCase));

            RemplirInstitutionnelComplet(d);
        }

        [Test]
        public void RemplissageInstitutionEntreprise()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "institutions.json"));
            Assert.That(File.Exists(path), $"Fichier introuvable : {path}");

            var d = JsonLoader.LoadArray<InstitutionData>(path)
                              .First(x => string.Equals(x.OrganizationType?.Trim(), "Entreprise", StringComparison.OrdinalIgnoreCase));

            RemplirInstitutionnelComplet(d);
        }

        // --------- Méthode unique : communs + spécifiques Institutionnel ----------
        private void RemplirInstitutionnelComplet(InstitutionData d)
        {
            // Champs communs (email, mdp, identité, pays, tel, nationalité, etc.)
            FillCommonFields(d);

            // “Vous êtes : Institutionnel”
            var instFor = Driver.FindElement(By.XPath("//fieldset[.//legend[contains(normalize-space(.),'Vous êtes')]]//label[normalize-space(.)='Institutionnel']")).GetAttribute("for");
            var instRadio = Driver.FindElement(By.Id(instFor));
            if (!instRadio.Selected) instRadio.SendKeys(Keys.Space);

            // Fonction
            var fonctionInput = Driver.FindElement(By.Id("edit-field-fonction-0-value"));
            fonctionInput.SendKeys(d.Function);

            // Type d'organisme (Selectize)
            var typeFor = Driver.FindElement(By.XPath("//label[contains(normalize-space(.),\"Type d'organisme\")]")).GetAttribute("for");
            ((IJavaScriptExecutor)Driver).ExecuteScript("document.getElementById(arguments[0]).focus();", typeFor);
            Driver.SwitchTo().ActiveElement().SendKeys(Keys.Backspace);
            Driver.SwitchTo().ActiveElement().SendKeys(d.OrganizationType + Keys.Enter);

            // Nom de l'organisme
            var orgNameInput = Driver.FindElement(By.Id("edit-field-nom-organisme-0-value"));
            orgNameInput.SendKeys(d.OrganizationName);

            // --- Assertions ---
            var emailVal = Driver.FindElement(By.XPath("//label[normalize-space(.)='Mon adresse e-mail']/following::input[1]"))
                                .GetAttribute("value") ?? "";

            var typeSelectId = (typeFor ?? "edit-field-type-organisme-selectized").Replace("-selectized", "");
            var orgTypeText  = Driver.FindElement(By.CssSelector("#" + typeSelectId + " + .selectize-control .selectize-input .item"))
                                    .Text ?? "";

            bool okEmail = emailVal == d.Email;
            bool okRadio = instRadio.Selected;
            bool okType  = orgTypeText == d.OrganizationType;

            TestContext.WriteLine($"[Vous êtes] Institutionnel sélectionné={instRadio.Selected}");
            TestContext.WriteLine($"[Type organisme] lu='{orgTypeText}' attendu='{d.OrganizationType}'");

            // --- Log JSON via ResultWriter ---
            void Log(string check, string expected, string actual, bool passed)
            {
                ResultWriter.Append(new {
                    test = TestContext.CurrentContext.Test.Name,
                    category = "Institutions",
                    check, expected, actual, passed,
                    ts = DateTime.UtcNow.ToString("o")
                });
            }

            Log("Email", d.Email, emailVal, okEmail);
            Log("Vous êtes", "Institutionnel", instRadio.Selected.ToString(), okRadio);
            Log("Type d'organisme", d.OrganizationType, orgTypeText, okType);

            // --- Asserts ---
            Assert.Multiple(() =>
            {
                Assert.That(okEmail, $"[Email] attendu='{d.Email}' obtenu='{emailVal}'");
                Assert.That(okRadio, "[Vous êtes] 'Institutionnel' devrait être sélectionné");
                Assert.That(okType,  $"[Type organisme] attendu='{d.OrganizationType}' obtenu='{orgTypeText}'");
            });
        }
    }
}
