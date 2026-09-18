using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private object carriedEntity;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        playerMovement.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        playerMovement.OnInteractPressed -= HandleInteractPressed;
    }

    private void HandleInteractPressed()
    {
        // TODO: carriedEntity 유무에 따라 들기/놓기 분기
    }
}
