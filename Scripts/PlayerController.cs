using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    private float speed = 5.0f;
    private bool isAxing = false;
    private Rigidbody2D rigidbody2d;
    private Vector2 moveInput;
    private Vector2 lookDirection;
    private Animator animator;
    private HairController hairController;
    private ToolController toolController;
    private TreeController tree;
    [SerializeField] private UIInventoryManager inventoryManager;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction ToggleInventoryAction;
    [SerializeField] private InputAction ChoppingAction;

    public Animator hairAnimator;
    public Animator toolAnimator;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        ToggleInventoryAction.Enable();
        ChoppingAction.Enable();

        ToggleInventoryAction.performed += OnToggleInventoryPerformed;
        ChoppingAction.performed += ChopTree;
    }

    void OnDisable()
    {
        ToggleInventoryAction.performed -= OnToggleInventoryPerformed;
        ChoppingAction.performed -= ChopTree;

        ToggleInventoryAction.Disable();
        ChoppingAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        hairController = GetComponentInChildren<HairController>();
        toolController = GetComponentInChildren<ToolController>();
        hairController.ChangeHairStyle("curlyhair_idle_strip9_0");
        toolController.ChangeTool("tools_idle_strip9_0");
        moveAction.Enable();
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<UIInventoryManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAxing)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            if (!Mathf.Approximately(moveInput.x, 0.0f))
            {
                lookDirection.Set(moveInput.x, 0f);
                lookDirection.Normalize();
            }
            Set_Look(new Animator[] { animator, hairAnimator, toolAnimator });
            Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });

        }
    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + moveInput * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    // Animation event
    void OnAxeComplete()
    {
        isAxing = false;
        speed = 5.0f;
    }

    // Shake the tree by Animation event
    void ShakeTree()
    {
        if (tree != null && !tree.isCut)
        {
            tree.ShakeTree();
        } else
        {
            Debug.Log("Tree is null to shake");
        }
    }

    void ChopTree(InputAction.CallbackContext context)
    {
        // Axe animation
        moveInput = Vector2.zero;
        Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });
        toolController.ChangeTool("tools_axe_strip10_0");
        Set_Trigger(new Animator[] { animator, hairAnimator, toolAnimator }, "AxeTrigger");
        isAxing = true;

        Collider2D hit = Physics2D.OverlapBox(rigidbody2d.position + new Vector2(lookDirection.x * 1.0f, 0.2f), new Vector2(1.0f, 0.8f), 0, LayerMask.GetMask("Tree"));
        if (hit != null)
        {
            tree = hit.GetComponent<TreeController>();
            if (tree != null && !tree.isCut)
            {
                tree.HitTree();
                Debug.Log("Tree found! Health: " + tree.health);
            }
        } else {
            tree = null;
        }
    }

    void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ToggleInventory();
        } else
        {
            Debug.Log("InventoryManager is null");
        }
    }

    void Set_Look(Animator[] animators)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat("Look X", lookDirection.x);
        }
    }

    void Set_Speed(Animator[] animators)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }
    }

    void Set_Trigger(Animator[] animators, string trigger)
    {
        foreach (Animator animator in animators)
        {
            animator.SetTrigger(trigger);
        }
    }
}
