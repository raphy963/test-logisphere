using NUnit.Framework;
using Moq;
using Librairie.Entities;
using Librairie.Services;
using Librairie.Services.Interfaces;
using System;

namespace LibrairieTest.Services
{
    public class LivreServiceTests
    {
        [Test]
        [Category("Book buy")]
        public void Checks_If_The_Price_Is_More_Than_Zero_Before_Buying()
        {
            // Setup
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            LivreService livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception exNegativeNumber = Assert.Throws<ArgumentException>(() =>
            {
                livreService.AcheterLivre(Guid.NewGuid(), Guid.NewGuid(), -2);
            });
            Exception exZero = Assert.Throws<ArgumentException>(() =>
            {
                livreService.AcheterLivre(Guid.NewGuid(), Guid.NewGuid(), 0);
            });

            // Assert
            Assert.That(exNegativeNumber.Message, Is.EqualTo("Montant must be higher than zero"));
            Assert.That(exZero.Message, Is.EqualTo("Montant must be higher than zero"));
        }
        [Test]
        [Category("Book buy")]
        public void Checks_If_Client_Exists_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => null); // Client not found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                livreService.AcheterLivre(clientGuid, livreGuid, 10);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Client does not exist"));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_Livre_Exists_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => null); // Livre not found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                livreService.AcheterLivre(clientGuid, livreGuid, 10);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Book does not exist"));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_Livre_Has_Copies_In_Stock_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => new Livre()
            {
                Id = livreGuid,
                Quantite = 0
            }); // Livre found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<InvalidOperationException>(() =>
            {
                livreService.AcheterLivre(clientGuid, livreGuid, 10);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Quantity of book is 0"));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_Montant_Is_Equal_Or_Higher_Than_Livre_Price_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => new Livre()
            {
                Id = livreGuid,
                Quantite = 1,
                Prix = 10
            }); // Livre found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<InvalidOperationException>(() =>
            {
                livreService.AcheterLivre(clientGuid, livreGuid, 9);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Montant is not enough for book's price"));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_The_Book_Quantity_Updates_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Livre testLivre = new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            }; // Livre found
            Client testClient = new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }; // Client found

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => testClient);
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => testLivre);
            serviceBd.Setup(m => m.ModifierLivre(It.Is<Livre>(l => l.Id == livreGuid && l.Quantite == 1))); // Updated quantity is sent to DB
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            livreService.AcheterLivre(clientGuid, livreGuid, 20);

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups.
            Assert.That(testLivre.Quantite, Is.EqualTo(1));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_The_Book_Is_Added_To_The_Client_List_When_Buying()
        {
            // Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Livre testLivre = new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            }; // Livre found
            Client testClient = new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }; // Client found

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => testLivre);
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => testClient);
            serviceBd.Setup(m => m.ModifierLivre(It.Is<Livre>(l => l.Id == livreGuid && l.Quantite == 1))); // Updated quantity is sent to DB
            serviceBd.Setup(m => m.ModifierClient(It.Is<Client>(c => c.Id == clientGuid && c.ListeLivreAchete.ContainsKey(livreGuid)))); // Client's list is updated
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            livreService.AcheterLivre(clientGuid, livreGuid, 20);

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups. 
            Assert.That(testClient.ListeLivreAchete, Does.ContainKey(livreGuid));
        }

        [Test]
        [Category("Book buy")]
        public void Checks_If_The_Due_Amount_Is_Properly_Calculated_When_Buying()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            }); // Livre found
            serviceBd.Setup(m => m.ModifierLivre(It.IsAny<Livre>())); // DB Doesn't matter in this test
            serviceBd.Setup(m => m.ModifierClient(It.IsAny<Client>())); // DB Doesn't matter in this test

            var livreService = new LivreService(serviceBd.Object);

            // Execute
            decimal dueAmount = livreService.AcheterLivre(clientGuid, livreGuid, 20);

            // Assert
            Assert.That(dueAmount, Is.EqualTo(10));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_Client_Exists_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => null); // Client not found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                livreService.RembourserLivre(clientGuid, livreGuid);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Client does not exist"));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_Livre_Exists_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => null); // Livre not found
            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                livreService.RembourserLivre(clientGuid, livreGuid);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Book does not exist"));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_Client_Has_The_Book_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test"
            }); // Empty list of book
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            }); // Livre found

            var livreService = new LivreService(serviceBd.Object);

            // Execute
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                livreService.RembourserLivre(clientGuid, livreGuid);
            });

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(ex.Message, Is.EqualTo("Client does not own the book"));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_Book_Inventory_Is_Updated_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Livre testLivre = new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            };
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test",
                ListeLivreAchete = new Dictionary<Guid, int> {
                    { livreGuid, 1 }
                }
            }); // Client found with the book bought
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => testLivre); // Livre found
            serviceBd.Setup(m => m.ModifierLivre(It.Is<Livre>(l => l.Id == livreGuid && l.Quantite == 3))); // Updated quantity is sent to DB
            serviceBd.Setup(m => m.ModifierClient(It.IsAny<Client>())); // Client update does not matters

            var livreService = new LivreService(serviceBd.Object);

            // Execute
            livreService.RembourserLivre(clientGuid, livreGuid);

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(testLivre.Quantite, Is.EqualTo(3));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_Book_Is_Removed_From_Customer_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuidOneCopy = Guid.NewGuid();
            Guid livreGuidTwoCopy = Guid.NewGuid();
            Livre testLivreOneCopy = new Livre()
            {
                Id = livreGuidOneCopy,
                Quantite = 2,
                Prix = 10
            };
            Livre testLivreTwoCopy = new Livre()
            {
                Id = livreGuidTwoCopy,
                Quantite = 2,
                Prix = 10
            };
            Client testClient = new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test",
                ListeLivreAchete = new Dictionary<Guid, int> {
                    { livreGuidOneCopy, 1 },
                    { livreGuidTwoCopy, 2 },
                }
            };

            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => testClient); // Client found with the book bought
            serviceBd.Setup(m => m.ObtenirLivre(livreGuidOneCopy)).Returns(() => testLivreOneCopy); // Livre with one copy owned by client found
            serviceBd.Setup(m => m.ObtenirLivre(livreGuidTwoCopy)).Returns(() => testLivreTwoCopy); // Livre with one copy owned by client found
            serviceBd.Setup(m => m.ModifierLivre(It.IsAny<Livre>())); // Livre update does not matters
            serviceBd.Setup(m => m.ModifierClient(It.Is<Client>(c => c.Id == clientGuid)));

            var livreService = new LivreService(serviceBd.Object);

            // Execute
            livreService.RembourserLivre(clientGuid, livreGuidTwoCopy); // Two copy logic
            livreService.RembourserLivre(clientGuid, livreGuidOneCopy); // One copy logic

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(testClient.ListeLivreAchete, Does.Not.ContainKey(livreGuidOneCopy));
            Assert.That(testClient.ListeLivreAchete, Does.ContainKey(livreGuidTwoCopy).WithValue(1));
        }

        [Test]
        [Category("Book refund")]
        public void Checks_If_The_Right_Amount_To_Refund_Is_Given_When_Refunding()
        {
            //Setup
            Guid clientGuid = Guid.NewGuid();
            Guid livreGuid = Guid.NewGuid();
            Livre testLivre = new Livre()
            {
                Id = livreGuid,
                Quantite = 2,
                Prix = 10
            };
            Client testClient = new Client()
            {
                Id = clientGuid,
                NomUtilisateur = "test",
                ListeLivreAchete = new Dictionary<Guid, int> {
                    { livreGuid, 1 }
                }
            };

            // Mock
            Mock<IServiceBD> serviceBd = new Mock<IServiceBD>();
            serviceBd.Setup(m => m.ObtenirClient(clientGuid)).Returns(() => testClient); // Client found with the book bought
            serviceBd.Setup(m => m.ObtenirLivre(livreGuid)).Returns(() => testLivre); // Livre found
            serviceBd.Setup(m => m.ModifierLivre(It.IsAny<Livre>())); // Livre update does not matters
            serviceBd.Setup(m => m.ModifierClient(It.IsAny<Client>())); // Client update does not matters

            var livreService = new LivreService(serviceBd.Object);

            // Execute
            decimal refundAmount = livreService.RembourserLivre(clientGuid, livreGuid);

            // Assert
            serviceBd.VerifyAll(); // Will verify all mocked setups
            Assert.That(refundAmount, Is.EqualTo(10));
        }
    }
}
