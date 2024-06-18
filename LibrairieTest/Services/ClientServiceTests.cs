using NUnit.Framework;
using Moq;
using Librairie.Entities;
using Librairie.Services;
using Librairie.Services.Interfaces;

namespace LibrairieTest.Services
{
    public class ClientServiceTests
    {
        [Test]
        [Category("User creation")]
        public void Checks_For_An_Username_When_Creating()
        {
            // Mock
            Mock<IServiceBD> serviceBD = new Mock<IServiceBD>();
            ClientService clientService = new ClientService(serviceBD.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                clientService.CreerClient("");
            });

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username must be specified"));
        }

        [Test]
        [Category("User creation")]
        public void Checks_For_An_Existing_Username_When_Creating()
        {
            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient("test")).Returns(new Client()
            {
                Id = Guid.NewGuid(),
                NomUtilisateur = "test"

            });
            ClientService clientService = new ClientService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                clientService.CreerClient("test");
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Username must be unique"));
        }

        [Test]
        [Category("User creation")]
        public void A_Client_Is_Properly_Created()
        {
            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient("test")).Returns(() => null); // Check for an existing customer and return null (No existing user found)
            serviceBd.Setup(m => m.AjouterClient(It.Is<Client>(c => c.NomUtilisateur == "test"))); // Only chercking username, guid is too random to properly check and keep the service as it would normally work.
            ClientService clientService = new ClientService(serviceBd.Object);

            // Execute
            Client result = clientService.CreerClient("test");

            // Assert 
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(result.Id.ToString(), Is.Not.EqualTo("00000000-0000-0000-0000-000000000000")); // A new guid was properly created
            Assert.That(result.NomUtilisateur, Is.EqualTo("test")); // Username was properly set
            Assert.That(result.ListeLivreAchete, Is.Not.Null); // The class initializer properly work and created a new "Liste de livres achetés"
        }

        [Test]
        [Category("Rename user")]
        public void Checks_For_An_Username_When_Renaming()
        {
            // Mock
            Mock<IServiceBD> serviceBD = new Mock<IServiceBD>();
            ClientService clientService = new ClientService(serviceBD.Object);
            Guid guid = Guid.NewGuid();

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                clientService.RenommerClient(guid, "");
            });

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username must be specified"));
        }

        [Test]
        [Category("Rename user")]
        public void Checks_If_The_User_Exists_When_Renaming()
        {
            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(It.IsAny<Guid>())).Returns(() => null); // The requested guid does not exist
            ClientService clientService = new ClientService(serviceBd.Object);
            Guid guid = Guid.NewGuid();

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                clientService.RenommerClient(guid, "test");
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Client does not exist"));
        }

        [Test]
        [Category("Rename user")]
        public void Checks_If_The_Username_Is_Available_When_Renaming()
        {
            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(It.IsAny<Guid>())).Returns(() => new Client()); // The client is found
            serviceBd.Setup(m => m.ObtenirClient("test")).Returns(() => new Client()
            {
                Id = Guid.NewGuid(),
                NomUtilisateur = "test"
            }); // A client with the same username already exists
            ClientService clientService = new ClientService(serviceBd.Object);
            Guid guid = Guid.NewGuid();

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                clientService.RenommerClient(guid, "test");
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Username must be unique"));
        }

        [Test]
        [Category("Rename user")]
        public void Renaming_A_Client_Properly_Works()
        {
            // Setup
            Guid clientGuid = Guid.NewGuid(); // Creating a client to properly check the edit
            Client testClient = new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "oldName"
            };

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => testClient); // The client is found and returned.
            serviceBd.Setup(m => m.ObtenirClient("test")).Returns(() => null); // The username is unique
            serviceBd.Setup(m => m.ModifierClient(It.Is<Client>(c => c.Id == clientGuid && c.NomUtilisateur == "test"))); // The proper client and new username is sent to be edited
            ClientService clientService = new ClientService(serviceBd.Object);

            // Execute
            clientService.RenommerClient(clientGuid, "test");

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups.
            Assert.That(testClient.NomUtilisateur, Is.EqualTo("test"));
        }
    }
}