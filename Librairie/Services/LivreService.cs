using Librairie.Entities;
using Librairie.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librairie.Services
{
    public class LivreService : IServiceLivre
    {
        private IServiceBD _serviceBD;

        public LivreService(IServiceBD serviceBD)
        {
            this._serviceBD = serviceBD;
        }

        public decimal AcheterLivre(Guid IdClient, Guid IdLivre, decimal montant)
        {
            // Montant must be tangible
            if (montant <= 0)
                throw new ArgumentException("Montant must be higher than zero");

            // Check if client exists
            Client foundClient = _serviceBD.ObtenirClient(IdClient);
            if (foundClient == null)
                throw new ArgumentException("Client does not exist");

            // Check if livre exists
            Livre foundLivre = _serviceBD.ObtenirLivre(IdLivre);
            if (foundLivre == null)
                throw new ArgumentException("Book does not exist");

            // Arguments are good
            // Checking if the book can be sold
            if (foundLivre.Quantite == 0)
                throw new InvalidOperationException("Quantity of book is 0");

            // Checking if the given amount of money is enough to cover the book's cost
            if (montant < foundLivre.Prix)
                throw new InvalidOperationException("Montant is not enough for book's price");

            // Operation is valid, now executing
            foundLivre.Quantite--;

            // Check if the client already has the book, no judgement here, they might really like it :)
            if (foundClient.ListeLivreAchete.ContainsKey(foundLivre.Id))
                foundClient.ListeLivreAchete[foundLivre.Id]++;
            else
                foundClient.ListeLivreAchete.Add(foundLivre.Id, 1);

            // Updating database
            _serviceBD.ModifierLivre(foundLivre);
            _serviceBD.ModifierClient(foundClient);

            // Returning due amount of money
            return montant - foundLivre.Prix;
        }

        public decimal RembourserLivre(Guid IdClient, Guid idLivre)
        {
            // Check if client exists
            Client foundClient = _serviceBD.ObtenirClient(IdClient);
            if (foundClient == null)
                throw new ArgumentException("Client does not exist");

            // Check if livre exists
            Livre foundLivre = _serviceBD.ObtenirLivre(idLivre);
            if (foundLivre == null)
                throw new ArgumentException("Book does not exist");

            // Check if the client has the book
            if (foundClient.ListeLivreAchete.ContainsKey(idLivre) == false)
                throw new ArgumentException("Client does not own the book");

            // Arguments are valid and operation is good to go
            // Removing book from client
            if (foundClient.ListeLivreAchete[idLivre] > 1)
            {
                // The client owns multiple book but is returning one copy, simply remove one
                // CONTEXTE DU TEST : J'ai remarqué que mon test initial ne marcherait pas en réalisant ceci :)
                foundClient.ListeLivreAchete[idLivre]--;
            }
            else
            {
                // The client own the book only one time, remove it entirely.
                foundClient.ListeLivreAchete.Remove(idLivre);
            }

            // Updating the quantity of the book
            foundLivre.Quantite++;

            // Saving to DB
            _serviceBD.ModifierLivre(foundLivre);
            _serviceBD.ModifierClient(foundClient);

            return foundLivre.Prix;
        }
    }
}
