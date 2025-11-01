// Créer le service qui va gérer les publications des messages vers l'exterieur
namespace Teams.INFRA.Layer.Models
{ 
    public class TeamMessage
    {
        public string Action { get; set; }
        public string TeamName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    }
}
