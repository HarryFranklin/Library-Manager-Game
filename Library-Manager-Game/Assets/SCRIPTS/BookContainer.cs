using UnityEngine;

public class BookContainer : Placeable
{
    [Header("Inventory Settings")]
    public int maxBooks = 10;
    public int currentBooks = 10; 

    public override bool HasAvailableSlot()
    {
        return currentBooks > 0 && base.HasAvailableSlot();
    }

    public bool TryTakeBook()
    {
        if (currentBooks > 0)
        {
            currentBooks--;
            Debug.Log($"{gameObject.name}: Book taken. {currentBooks} remaining.");
            return true;
        }
        return false;
    }

    public bool NeedsRestocking()
    {
        return currentBooks < maxBooks;
    }

    public int GetMissingBookCount()
    {
        return maxBooks - currentBooks;
    }

    public void AddBooks(int amount)
    {
        currentBooks += amount;
        currentBooks = Mathf.Clamp(currentBooks, 0, maxBooks);
        Debug.Log($"{gameObject.name}: Shelf restocked. Now holds {currentBooks}/{maxBooks} books.");
    }
}