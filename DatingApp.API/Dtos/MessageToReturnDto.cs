using System;


namespace DatingApp.API.Dtos
{
    public class MessageToReturnDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }

        // automapper riesce a mappare correttamente in automatico perchè vede che SenderId è l'id da cui Sender è la navigation property
        // e se nella classe user c'è una proprietà che chiama KnownAs mappa quella sulla property SenderKonwnAs
        public string SenderKnownAs { get; set; } 
        public string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public string RecipientKnownAs { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string Content { get; set; } 
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
    }
}