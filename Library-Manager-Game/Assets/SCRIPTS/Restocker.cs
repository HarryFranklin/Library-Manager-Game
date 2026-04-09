using UnityEngine;
using System.Collections;

public class Restocker : StaffAI
{
    public enum RestockerState { Idle, WalkingToStockDesk, CollectingBooks, WalkingToShelf, RestockingShelf }

    [Header("State")]
    public RestockerState currentState = RestockerState.Idle;

    [Header("Inventory")]
    public int currentlyCarriedBooks = 0;

    [Header("Timers")]
    public float timePerBookCollected = 0.5f;
    public float timePerBookShelved = 0.5f;

    private StockDesk targetStockDesk;
    private BookContainer targetShelf;

    // RPG Integration: Carrying capacity increases with level
    public int GetCarryCapacity()
    {
        // Level 1 = 3 books, Level 2 = 4 books, Level 5 = 7 books
        return 2 + currentLevel; 
    }

    private void Update()
    {
        switch (currentState)
        {
            case RestockerState.Idle:
                // Periodically scan for work to save performance
                if (Time.frameCount % 30 == 0) FindWork();
                break;

            case RestockerState.WalkingToStockDesk:
                if (HasReachedDestination()) ChangeState(RestockerState.CollectingBooks);
                break;

            case RestockerState.WalkingToShelf:
                if (HasReachedDestination()) ChangeState(RestockerState.RestockingShelf);
                break;
        }
    }

    private void FindWork()
    {
        // 1. Find the shelf that is missing the MOST books
        GameObject[] shelves = GameObject.FindGameObjectsWithTag("BookContainer");
        BookContainer emptiestShelf = null;
        int highestMissingCount = 0;

        foreach (GameObject obj in shelves)
        {
            if (obj.TryGetComponent(out BookContainer shelf))
            {
                if (shelf.NeedsRestocking())
                {
                    int missing = shelf.GetMissingBookCount();
                    if (missing > highestMissingCount)
                    {
                        highestMissingCount = missing;
                        emptiestShelf = shelf;
                    }
                }
            }
        }

        if (emptiestShelf == null) return; // No work to do right now

        // 2. Find the Stock Desk
        GameObject deskObj = GameObject.FindGameObjectWithTag("StockDesk");
        if (deskObj != null && deskObj.TryGetComponent(out StockDesk desk))
        {
            targetShelf = emptiestShelf;
            targetStockDesk = desk;

            // If we already have enough books in hand, go straight to the shelf
            if (currentlyCarriedBooks > 0)
            {
                ChangeState(RestockerState.WalkingToShelf);
            }
            else
            {
                ChangeState(RestockerState.WalkingToStockDesk);
            }
        }
        else
        {
            Debug.LogWarning("Restocker needs a StockDesk placed in the scene to gather books!");
        }
    }

    private void ChangeState(RestockerState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case RestockerState.WalkingToStockDesk:
                if (targetStockDesk != null && targetStockDesk.staffNode != null)
                {
                    MoveTo(targetStockDesk.staffNode.position);
                }
                break;

            case RestockerState.CollectingBooks:
                StartCoroutine(CollectBooksRoutine());
                break;

            case RestockerState.WalkingToShelf:
                if (targetShelf != null)
                {
                    // Walk to the shelf's first interaction node, or its centre if none exist
                    Transform node = targetShelf.interactionNodes.Count > 0 ? targetShelf.interactionNodes[0] : targetShelf.transform;
                    MoveTo(node.position);
                }
                break;

            case RestockerState.RestockingShelf:
                StartCoroutine(RestockShelfRoutine());
                break;
        }
    }

    private IEnumerator CollectBooksRoutine()
    {
        int capacity = GetCarryCapacity();
        int booksToGrab = capacity - currentlyCarriedBooks;

        for (int i = 0; i < booksToGrab; i++)
        {
            // Buy wholesale books for £1 each from the Economy
            if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney > 0)
            {
                EconomyManager.Instance.AddMoney(-1); 
                currentlyCarriedBooks++;
                yield return new WaitForSeconds(timePerBookCollected);
            }
            else if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney <= 0)
            {
                Debug.Log("Library is bankrupt! Cannot afford to restock books.");
                break; 
            }
            else
            {
                // Fallback if EconomyManager is missing
                currentlyCarriedBooks++;
                yield return new WaitForSeconds(timePerBookCollected);
            }
        }

        if (currentlyCarriedBooks > 0)
        {
            ChangeState(RestockerState.WalkingToShelf);
        }
        else
        {
            ChangeState(RestockerState.Idle);
        }
    }

    private IEnumerator RestockShelfRoutine()
    {
        // Only place as many books as the shelf is missing
        int missingBooks = targetShelf.GetMissingBookCount();
        int booksToPlace = Mathf.Min(missingBooks, currentlyCarriedBooks);

        for (int i = 0; i < booksToPlace; i++)
        {
            targetShelf.AddBooks(1);
            currentlyCarriedBooks--;
            
            // Gain 1 XP per book successfully shelved
            GainXP(1);

            yield return new WaitForSeconds(timePerBookShelved);
        }

        // Job done. Reset and look for the next empty shelf.
        targetShelf = null;
        ChangeState(RestockerState.Idle);
    }
}