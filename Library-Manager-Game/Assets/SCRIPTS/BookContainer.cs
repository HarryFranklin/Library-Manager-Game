using UnityEngine;

public class BookContainer : Placeable
{
    [Header("Inventory Settings")]
    public int maxBooks = 10;
    public int currentBooks = 10; 

    // Override the base availability to ALSO check if we have books
    public override bool HasAvailableSlot()
    {
        // Must have books AND have a physical slot free
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
}