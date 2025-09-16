using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class CharacterSelectable : MonoBehaviour
{
    [Header("Identidad")]
    public string characterId = "Player1";

    [Header("Animación")]
    [SerializeField] private Animator animator;              // arrastra el Animator del modelo
    [SerializeField] private string idleStateName = "Idle";  // estado Idle del controller
    [SerializeField] private string selectStateName = "Dance"; // <- tu clip/estado de baile
    [SerializeField] private float crossfade = 0.1f;

    [Header("Visual de selección (opcional)")]
    public GameObject selectedHighlight;

    [SerializeField] private CharacterSelectionServiceMono selectionService;

    private int _idleHash;
    private int _selectHash;

    private void Awake()
    {
        if (selectionService == null) selectionService = FindObjectOfType<CharacterSelectionServiceMono>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        _idleHash = Animator.StringToHash(idleStateName);
        _selectHash = Animator.StringToHash(selectStateName);
    }

    private void OnEnable()
    {
        if (selectionService != null) selectionService.OnSelectedChanged += OnSelectionChanged;
        OnSelectionChanged(selectionService != null ? selectionService.SelectedId : null);
    }

    private void OnDisable()
    {
        if (selectionService != null) selectionService.OnSelectedChanged -= OnSelectionChanged;
    }

    private void OnMouseDown()              // click/tap (requiere Collider + Camera)
    {
        if (selectionService != null) selectionService.Select(characterId);
    }

    private void OnSelectionChanged(string currentId)
    {
        bool iAmSelected = (currentId == characterId);

        if (selectedHighlight) selectedHighlight.SetActive(iAmSelected);

        if (animator != null)
        {
            // Si estoy seleccionado → reproducir Dance; si no → volver a Idle
            animator.CrossFade(iAmSelected ? _selectHash : _idleHash, crossfade, 0);
        }
    }
}