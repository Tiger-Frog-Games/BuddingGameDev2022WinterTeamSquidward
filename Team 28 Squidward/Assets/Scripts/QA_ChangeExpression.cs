using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QA_ChangeExpression : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRender;

    [SerializeField] private Sprite[] expressions;
    private int activeIndex = 0;

    public void nextExpression()
    {
        if (expressions.Length == 0)
        {
            return;
        }

        activeIndex++;
        if (activeIndex >= expressions.Length)
        {
            activeIndex = 0;
        }

        spriteRender.sprite = expressions[activeIndex];

    }
}
