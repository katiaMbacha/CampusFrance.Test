namespace CampusFrance.Test.DataManagement
{
    // ---------- Commun à tous ----------
    public class RegistrationBaseData
    {
        // Connexion
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordConfirm { get; set; } = "";

        // Informations personnelles (communes)
        public string Civility { get; set; } = "";      // "Mme" | "Mr" | "Dr" | etc.
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string CountryResidence { get; set; } = "";
        public string Nationality { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string City { get; set; } = "";
        public string Phone { get; set; } = "";

        // Consentement communications
        public bool AcceptComms { get; set; } = true;
    }

    // ---------- Étudiants ----------
    public class StudentData : RegistrationBaseData
    {
        public string StudyField { get; set; } = "";    // Domaine d'études
        public string StudyLevel { get; set; } = "";    // Niveau(x) d'étude
    }

    // ---------- Chercheurs ----------
    public class ResearcherData : RegistrationBaseData
    {
        public string StudyField { get; set; } = "";
        public string StudyLevel { get; set; } = "";
    }

    // ---------- Institutionnels ----------
    public class InstitutionData : RegistrationBaseData
    {
        public string Function { get; set; } = "";          // Fonction
        public string OrganizationType { get; set; } = "";  // Type d'organisme
        public string OrganizationName { get; set; } = "";      // Nom de l'organisme
    }
}
