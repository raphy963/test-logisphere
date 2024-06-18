using System;
using System.Collections.Generic;

namespace Questions
{
    // Améliorer le code de cette classe, ainsi que sa relation avec la classe Collaborateur.
    public class Question
    {
        public void Traiter(List<string> listeContenu)
        {
            // Collaborateur 
            List<string> listeContenuValide = new List<string>();
            List<string> errors = new List<string>();

            foreach(string contenu in listeContenu)
            {
                try
                {
                    Valider(contenu);
                    listeContenuValide.Add(contenu);
                }
                catch (Exception ex)
                {
                    // Exception retournée de la validation.
                    errors.Add(ex.Message);
                    continue; // Erreur sauvegardé, on continue la boucle
                }  
            }

            // Ici : Faire quelque chose avec la liste "Errors" pour du error management approprié.

            if(listeContenuValide.Count > 0)
            {
                listeContenuValide.ForEach(c => Collaborateur.AjouterContenuBD(c));
            }
        }

        private void Valider(string contenu)
        {
            if (contenu == "")
                throw new Exception("Le contenu ne peut être vide");

            if (contenu.Length > 10)
                throw new Exception("Le contenu est trop long");
        }
    }
}
