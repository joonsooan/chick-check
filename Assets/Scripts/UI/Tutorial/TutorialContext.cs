public class TutorialContext
{
    public PlayerMovement PlayerMovement { get; }
    public PlayerCarry PlayerCarry { get; }
    public Mirror Mirror { get; }
    public TutorialPopup Popup { get; }

    public TutorialContext(PlayerMovement playerMovement, PlayerCarry playerCarry, Mirror mirror, TutorialPopup popup)
    {
        PlayerMovement = playerMovement;
        PlayerCarry = playerCarry;
        Mirror = mirror;
        Popup = popup;
    }
}
