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
            throw new NotImplementedException();
        }

        public void RenommerClient(Guid clientId, string nouveauNomClient)
        {
            throw new NotImplementedException();
        }
    }
}
