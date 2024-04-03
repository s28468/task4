using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserData(firstName, lastName, email, dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            AssignCreditLimit(client, user);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
        private static bool ValidateUserData(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            return !string.IsNullOrEmpty(firstName) &&
                   !string.IsNullOrEmpty(lastName) &&
                   email.Contains("@") &&
                   email.Contains(".") &&
                   CalculateAge(dateOfBirth) >= 21;
        }
        private static int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;
            return age;
        }
        private static User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }
        private void AssignCreditLimit(Client client, User user)
        {
            // convert from string ClientType 
            var clientType = DetermineClientType(client);
            
            switch (clientType)
            {
                case ClientType.VeryImportant:
                    user.HasCreditLimit = false;
                    break;

                case ClientType.Important:
                    SetCreditLimit(user, factor: 2);
                    break;

                default: // Standard
                    SetCreditLimit(user, factor: 1);
                    break;
            }
        }
        private enum ClientType
        {
            Standard,
            Important,
            VeryImportant
        }
        private ClientType DetermineClientType(Client client)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    return ClientType.VeryImportant;
                case "ImportantClient":
                    return ClientType.Important;
                default:
                    return ClientType.Standard;
            }
        }
        
        private void SetCreditLimit(User user, int factor)
        {
            using (var userCreditService = new UserCreditService())
            {
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = creditLimit * factor;
            }
            user.HasCreditLimit = true;
        }


    }
}
