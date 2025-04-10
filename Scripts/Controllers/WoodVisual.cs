using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WoodVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sequence bounceSequence;
    private float flyDistance = 1.5f; // Distance to fly up
    private float pickupDistance = 1.0f; // Distance to pick up the wood
    private float fadeTime = 0.2f;
    [SerializeField] private ItemDefinition woodDefinition; // Reference to the wood item definition

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0, 0.2f)) * flyDistance;
        Vector3 targetPosition = transform.position + (Vector3)randomDirection;

        bounceSequence = DOTween.Sequence();

        bounceSequence.Append(transform.DOJump(targetPosition, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad));
        bounceSequence.Append(transform.DOJump(targetPosition, 0.2f, 1, 0.2f).SetEase(Ease.OutQuad));
        bounceSequence.Append(transform.DOJump(targetPosition, 0.1f, 1, 0.1f).SetEase(Ease.OutQuad));
    }

    void Update()
    {
        if (PlayerController.Instance == null) return; // Check if PlayerController is available

        float sqrDistance = (transform.position - PlayerController.Instance.transform.position).sqrMagnitude; // Calculate squared distance
        if (sqrDistance < pickupDistance * pickupDistance)
        {
            enabled = false;

            // Fade + scale
            transform.DOScale(0f, fadeTime).SetEase(Ease.InBack);
            spriteRenderer.DOFade(0f, fadeTime).OnComplete(() =>
            {
                PlayerInventory.Instance.AddItem(woodDefinition, 1);
                Destroy(gameObject);
            });
        }
    }

    void OnDestroy()
    {
        bounceSequence.Kill();
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);
    }
}
