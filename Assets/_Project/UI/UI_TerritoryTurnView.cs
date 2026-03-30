using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_TerritoryTurnView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private TMP_Text nextDayButtonText;
    [SerializeField] private GameObject staminaPrefab;
    [SerializeField] private Transform staminaContainer;

    [Header("Stamina Colors")]
    [SerializeField] private Color activeStaminaColor = Color.white;
    [SerializeField] private Color inactiveStaminaColor = new Color(1f, 1f, 1f, 0.28f);

    private readonly List<GameObject> _staminaObjects = new List<GameObject>();
    private readonly List<Graphic> _staminaGraphics = new List<Graphic>();
    private bool _isBound;
    private int _currentSlotCount;

    public event Action NextDayClicked;

    private void Awake()
    {
        if (nextDayButtonText == null && nextDayButton != null)
        {
            nextDayButtonText = nextDayButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Bind()
    {
        if (_isBound)
        {
            return;
        }

        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(HandleNextDayClicked);
        }

        if (nextDayButtonText != null)
        {
            nextDayButtonText.text = "다음 날";
        }

        _isBound = true;
    }

    // 날짜, 남은 도전 횟수, 다음 날 버튼 활성 상태를 HUD에 반영한다.
    public void Refresh(TerritoryTurnState turnState)
    {
        if (turnState == null)
        {
            return;
        }

        SetDayText(turnState.CurrentDay);
        EnsureStaminaSlots(turnState.MaxAttemptsPerDay);
        RefreshStamina(turnState.RemainingAttempts);

        if (nextDayButton != null)
        {
            nextDayButton.interactable = turnState.CanAdvanceDay;
        }
    }

    public void SetNextDayButtonInteractable(bool isInteractable)
    {
        if (nextDayButton != null)
        {
            nextDayButton.interactable = isInteractable;
        }
    }

    private void SetDayText(int day)
    {
        if (dayText != null)
        {
            dayText.text = $"{day}일차";
        }
    }

    private void EnsureStaminaSlots(int slotCount)
    {
        if (staminaPrefab == null || staminaContainer == null || _currentSlotCount == slotCount)
        {
            return;
        }

        ClearStaminaSlots();

        for (int index = 0; index < slotCount; index++)
        {
            GameObject staminaObject = Instantiate(staminaPrefab, staminaContainer);
            Graphic staminaGraphic = staminaObject.GetComponentInChildren<Graphic>(true);
            _staminaObjects.Add(staminaObject);
            _staminaGraphics.Add(staminaGraphic);
        }

        _currentSlotCount = slotCount;
    }

    private void RefreshStamina(int remainingAttempts)
    {
        for (int index = 0; index < _staminaGraphics.Count; index++)
        {
            Graphic staminaGraphic = _staminaGraphics[index];
            if (staminaGraphic == null)
            {
                continue;
            }

            staminaGraphic.color = index < remainingAttempts
                ? activeStaminaColor
                : inactiveStaminaColor;
        }
    }

    private void ClearStaminaSlots()
    {
        for (int index = 0; index < _staminaObjects.Count; index++)
        {
            GameObject staminaObject = _staminaObjects[index];
            if (staminaObject == null)
            {
                continue;
            }

            Destroy(staminaObject);
        }

        _staminaObjects.Clear();
        _staminaGraphics.Clear();
        _currentSlotCount = 0;
    }

    private void HandleNextDayClicked()
    {
        NextDayClicked?.Invoke();
    }
}
