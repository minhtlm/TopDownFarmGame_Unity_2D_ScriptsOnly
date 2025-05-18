using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TreeController : MonoBehaviour, IDestructible
{
    private Transform treeTopTransform;
    private Transform stumpTransform;
    private SpriteRenderer treeTopSpriteRenderer;
    private SpriteRenderer treeStumpSpriteRenderer;
    private Transform shadowTransform;
    private Sequence fallSequence;
    private Sequence shakeSequence;
    private bool isCut = false;
    private bool isFaded = false;
    private float tweeningDuration = 2.0f; // Duration for the tree to fall down
    private float sqrDistanceToFade; // Distance to fade the tree top
    private Vector3 centerTreeTop;
    private string toolType = "axe";
    [SerializeField] private int health = 8;
    [SerializeField] private int stumpHealth = 4;
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private int woodDropFromTree = 8;
    [SerializeField] private int woodDropFromStump = 4;
    [SerializeField] private AudioClip chopSound;
    
    void Awake()
    {
        stumpTransform = transform.Find("Stump");
        if (stumpTransform != null)
        {
            treeStumpSpriteRenderer = stumpTransform.GetComponent<SpriteRenderer>();
        }

        treeTopTransform = transform.Find("TreeTop");
        if (treeTopTransform != null)
        {
            treeTopSpriteRenderer = treeTopTransform.GetComponent<SpriteRenderer>();
            centerTreeTop = treeTopSpriteRenderer.bounds.center;
            sqrDistanceToFade = treeTopSpriteRenderer.bounds.extents.y * treeTopSpriteRenderer.bounds.extents.y;
        }
        shadowTransform = transform.Find("Shadow");
    }

    void Update()
    {
        if (treeTopSpriteRenderer == null) return;
        
        if (PlayerController.Instance != null)
        {
            Vector3 playerPosition = PlayerController.Instance.transform.position;
            if ((playerPosition - centerTreeTop).sqrMagnitude > sqrDistanceToFade)
            {
                if (isFaded)
                {
                    FadeOut();
                }
                return;
            }

            bool shouldFade = treeTopSpriteRenderer.bounds.Contains(playerPosition);

            if (shouldFade && !isFaded)
            {
                FadeIn();
            }
            else if (!shouldFade && isFaded)
            {
                FadeOut();
            }
        }
    }

    void FadeIn()
    {
        treeTopSpriteRenderer.DOFade(0.5f, 0.1f).SetEase(Ease.InQuad); // Fade the tree top
        treeStumpSpriteRenderer.DOFade(0.5f, 0.1f).SetEase(Ease.InQuad); // Fade the stump
        isFaded = true;
    }

    void FadeOut()
    {
        treeTopSpriteRenderer.DOFade(1f, 0.1f).SetEase(Ease.OutQuad); // Fade back the tree top
        treeStumpSpriteRenderer.DOFade(1f, 0.1f).SetEase(Ease.OutQuad); // Fade back the stump
        isFaded = false;
    }

    void OnDestroy()
    {
        DOTween.Kill(treeTopTransform);
        DOTween.Kill(stumpTransform);
        ResetSequence(shakeSequence);
        ResetSequence(fallSequence);
    }

    public bool CanInteract(string toolType)
    {
        return toolType == this.toolType;
    }

    public void Interact(string toolType)
    {
        if (!CanInteract(toolType)) return;
        
        HitTree();
    }

    public void OnInteractAnimation()
    {
        ShakeTree();
    }

    public void HitTree()
    {
        if (isCut)
        {
            stumpHealth--;
        }
        else
        {
            health--;
        }
    }

    public void ShakeTree()
    {
        PlayerController.Instance.PlaySound(chopSound); // Play the chop sound

        if (!isCut)
        {
            if (treeTopTransform != null)
            {
                ResetSequence(shakeSequence);
                shakeSequence = DOTween.Sequence();

                float angle = 2f; // Angle to rotate
                float duration = 0.08f; // Duration of each rotation

                // Shake the tree top
                shakeSequence.Append(treeTopTransform.DORotate(new Vector3(0, 0, angle), duration).SetRelative());
                shakeSequence.Append(treeTopTransform.DORotate(new Vector3(0, 0, -angle * 2), duration * 2).SetRelative());
                shakeSequence.Append(treeTopTransform.DORotate(new Vector3(0, 0, angle), duration).SetRelative());
                shakeSequence.Append(treeTopTransform.DORotate(Vector3.zero, duration));

                shakeSequence.Play();

                if (health <= 0)
                {
                    CutTreeTop();
                }
            }
        }
        else
        {
            if (stumpTransform != null)
            {
                stumpTransform.DOPunchPosition(new Vector3(0.05f, 0f, 0f), 0.3f, 6, 0.3f); // Punch the stump position

                if (stumpHealth <= 0) 
                {
                    SpawnWood(woodDropFromStump, stumpTransform.position);

                    DOTween.Kill(stumpTransform); // Stop any ongoing tweens on the stump

                    Destroy(gameObject);
                }
            }
        }
    }

    public void CutTreeTop()
    {
        isCut = true; // Set the tree as cut

        // Tween the tree to fall down
        TweenTreeTop();
    }

    private void TweenTreeTop()
    {
        if (treeTopTransform != null && shadowTransform != null)
        {
            Vector2 playerPosition = PlayerController.Instance.transform.position;
            float direction = (playerPosition.x > transform.position.x) ? 1f : -1f;
            float rotationAngle = direction * 90f;
            float treeTopHalf = treeTopSpriteRenderer.bounds.extents.y;
            Vector3 shadowTargetOffset;
            if (rotationAngle > 0) // đổ sang phải
                shadowTargetOffset = new Vector3(-treeTopHalf, 0, 0);
            else // đổ sang trái
                shadowTargetOffset = new Vector3(treeTopHalf, 0, 0);

            // Use sequence to rotate and move the tree top
            fallSequence = DOTween.Sequence();

            fallSequence.Append(treeTopTransform.DORotate(new Vector3(0, 0, rotationAngle), tweeningDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad)); // Rotate the tree top
            fallSequence.Join(treeTopTransform.DOMoveY(treeTopTransform.position.y, tweeningDuration).SetEase(Ease.InQuad)); // Move the tree top down
            fallSequence.Join(shadowTransform.DOMove(shadowTransform.position + shadowTargetOffset, tweeningDuration).SetEase(Ease.InQuad));

            fallSequence.OnComplete(() =>
            {
                if (treeTopTransform != null && treeTopSpriteRenderer != null)
                {
                    Vector3 spawnWoodPosition = treeTopSpriteRenderer.bounds.center + new Vector3(0, treeTopSpriteRenderer.bounds.extents.y, 0);

                    // After the tree has fallen, spawn wood and destroy the tree
                    SpawnWood(woodDropFromTree, spawnWoodPosition - new Vector3(0, 1f, 0));

                    ResetSequence(fallSequence);

                    Destroy(treeTopTransform.gameObject); // Destroy the tree top
                    Destroy(shadowTransform.gameObject); // Destroy the shadow
                }
            });
        }
    }

    private void SpawnWood(int amount, Vector3 spawnPos)
    {
        if (woodPrefab != null)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject wood = Instantiate(woodPrefab, spawnPos, Quaternion.identity);
                wood.transform.parent = null; // Unparent the wood object
            }
        }
    }

    private void ResetSequence(Sequence sequence)
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
    }
}
