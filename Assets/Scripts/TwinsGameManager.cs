using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class LevelData
{
    public int rows;
    public int columns;
}

public class TwinsGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Card Settings")]
    [SerializeField] private List<Sprite> cardSprites;

    [Header("Level Settings")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private int currentLevel = 0;

    private List<CardManager> allCards = new List<CardManager>();
    private Stack<CardManager> cardPool = new Stack<CardManager>();

    private CardManager firstCard, secondCard;
    private bool isProcessing;
    private int score;
    private int matchedPairs;
    private int totalPairs;

    private const string ScoreKey = "HighScore";

    private void Start()
    {
        LoadScore();
        InitializeCards();
    }

    private void InitializeCards()
    {
        LevelData level = levels[currentLevel];
        int rows = level.rows;
        int cols = level.columns;

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        AdjustCellSize(rows, cols);

        int totalCards = rows * cols;
        if (totalCards % 2 != 0) totalCards--; // Ensure even number of cards

        totalPairs = totalCards / 2;
        matchedPairs = 0;

        // Use only enough unique sprites
        int uniqueCount = Mathf.Min(totalPairs, cardSprites.Count);

        List<int> ids = new List<int>();
        Dictionary<int, Sprite> idToSprite = new Dictionary<int, Sprite>();

        for (int i = 0; i < totalPairs; i++)
        {
            int spriteIndex = i % uniqueCount;
            ids.Add(i);
            ids.Add(i);
            idToSprite[i] = cardSprites[spriteIndex];
        }

        Shuffle(ids);

        for (int i = 0; i < ids.Count; i++)
        {
            CardManager card = GetCardFromPool();
            card.transform.SetParent(cardGrid, false);
            card.Init(ids[i], idToSprite[ids[i]], this);
            allCards.Add(card);
        }
    }

    private void AdjustCellSize(int rows, int cols)
    {
        RectTransform rt = cardGrid.GetComponent<RectTransform>();

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;
        float paddingLeft = gridLayout.padding.left;
        float paddingRight = gridLayout.padding.right;
        float paddingTop = gridLayout.padding.top;
        float paddingBottom = gridLayout.padding.bottom;

        float totalWidth = rt.rect.width - paddingLeft - paddingRight - (spacingX * (cols - 1));
        float totalHeight = rt.rect.height - paddingTop - paddingBottom - (spacingY * (rows - 1));

        float cellWidth = totalWidth / cols;
        float cellHeight = totalHeight / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private CardManager GetCardFromPool()
    {
        if (cardPool.Count > 0)
        {
            var card = cardPool.Pop();
            card.gameObject.SetActive(true);
            return card;
        }

        return Instantiate(cardPrefab).GetComponent<CardManager>();
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public void OnCardClicked(CardManager clicked)
    {
        if (isProcessing || clicked == firstCard) return;

        clicked.FlipUp();
        AudioManager.Instance.PlayFlipSound();
        clicked.transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (firstCard == null)
        {
            firstCard = clicked;
        }
        else
        {
            secondCard = clicked;
            isProcessing = true;
            Invoke(nameof(CheckMatch), 0.5f);
        }
    }

    private void CheckMatch()
    {
        if (firstCard.GetID() == secondCard.GetID())
        {
            firstCard.Disable();
            secondCard.Disable();
            matchedPairs++;
            AudioManager.Instance.PlayMatchSound();
            UpdateScore(10);

            if (matchedPairs >= totalPairs)
            {
                Invoke(nameof(NextLevel), 1f);
            }
        }
        else
        {
            firstCard.FlipDown();
            secondCard.FlipDown();
            AudioManager.Instance.PlayFailSound();
            UpdateScore(-5);
        }

        firstCard = null;
        secondCard = null;
        isProcessing = false;
    }

    private void UpdateScore(int delta)
    {
        score += delta;
        score = Mathf.Max(0, score);
        scoreText.text = "Score: " + score;
        PlayerPrefs.SetInt(ScoreKey, score);
    }

    private void LoadScore()
    {
        score = PlayerPrefs.GetInt(ScoreKey, 0);
        scoreText.text = "Score: " + score;
    }


    private void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levels.Count)
            currentLevel = 0;

        ResetGame();
    }

    public void ResetGame()
    {
        foreach (var card in allCards)
        {
            card.gameObject.SetActive(false);
            cardPool.Push(card);
        }

        allCards.Clear();
        firstCard = secondCard = null;
        isProcessing = false;

        InitializeCards();
    }
}
