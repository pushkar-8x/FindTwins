using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    [SerializeField] private Button cardButton;

    private int cardId;
    private TwinsGameManager gameManager;

    public void Init(int id, Sprite faceSprite, TwinsGameManager manager)
    {
        cardId = id;
        faceImage.sprite = faceSprite;
        faceImage.enabled = false;
        gameManager = manager;
        cardButton.interactable = true;
    }

    public void OnClick()
    {
        gameManager.OnCardClicked(this);
    }

    public void FlipUp() => faceImage.enabled = true;
    public void FlipDown() => faceImage.enabled = false;
    public void Disable() => cardButton.interactable = false;

    public int GetID() => cardId;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnClick);
    }
}