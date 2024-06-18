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
            throw new NotImplementedException();
        }

        public decimal RembourserLivre(Guid IdClient, Guid idLivre)
        {
            throw new NotImplementedException();
        }
    }
}
