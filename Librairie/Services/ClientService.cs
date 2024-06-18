using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Librairie.Entities;
using Librairie.Services.Interfaces;

namespace Librairie.Services
{
    public class ClientService : IServiceClient
    {
        private IServiceBD _serviceBD;
        public ClientService(IServiceBD serviceBD)
        {
            this._serviceBD = serviceBD;
        }
        public Client CreerClient(string nomClient)
        {
            // Username must be valid (Not null, probably some more requirements in a real-life situation)
            if(nomClient == null || nomClient == "")
                throw new ArgumentException("Username must be specified");

            // Check if username exists
            if (_serviceBD.ObtenirClient(nomClient) != null)
                throw new ArgumentException("Username must be unique");

            // Good to go
            Client newClient = new Client()
            {
                Id = Guid.NewGuid(),
                NomUtilisateur = nomClient
            };
            _serviceBD.AjouterClient(newClient);

            return newClient;
        }

        public void RenommerClient(Guid clientId, string nouveauNomClient)
        {
            // Username must be valid (Not null, probably some more requirements in a real-life situation)
            if (nouveauNomClient == null || nouveauNomClient == "")
                throw new ArgumentException("Username must be specified");

            // Check if the provided clientId exists
            Client foundClient = _serviceBD.ObtenirClient(clientId);
            if (foundClient == null)
                throw new ArgumentException("Client does not exist");

            // Check if username exists
            if (_serviceBD.ObtenirClient(nouveauNomClient) != null)
                throw new ArgumentException("Username must be unique");

            // Good to go
            foundClient.NomUtilisateur = nouveauNomClient;
            _serviceBD.ModifierClient(foundClient);
        }
    }
}
