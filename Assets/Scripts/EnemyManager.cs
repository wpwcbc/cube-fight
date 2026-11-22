using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

[DefaultExecutionOrder(-1000)]
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    [Min(0.05f)] public float stepIntervalSeconds = 1f;
    public bool shuffleOrderEachTick = true;   // avoids bias; or use round-robin
    public bool blockSwaps = true;             // prevent A<->B edge swaps
    public bool allowEnterVacatedThisTick = true;

    public readonly HashSet<EnemyStepper> _enemies = new HashSet<EnemyStepper>();
    public int EnemyCount { get { return _enemies.Count; } }

    public event Action<int> OnEnemyCountChanged;
    public event Action OnAllEnemiesCleared;

    int _roundRobinOffset = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(TickLoop());
    }

    IEnumerator TickLoop()
    {
        var wait = new WaitForSeconds(stepIntervalSeconds);
        while (true)
        {
            StepOnce();
            yield return wait;
        }
    }

    struct Edge
    {
        public Vector2Int from, to;
        public Edge(Vector2Int f, Vector2Int t) { from = f; to = t; }
        public override int GetHashCode() => from.GetHashCode() ^ (to.GetHashCode() * 486187739);
        public override bool Equals(object obj) => obj is Edge e && e.from == from && e.to == to;
    }

    public bool Register(EnemyStepper e)
    {
        if (e == null || !e.gameObject.activeInHierarchy) return false;
        bool added = _enemies.Add(e);
        if (added)
        {
            int count = _enemies.Count;
            if (OnEnemyCountChanged != null) OnEnemyCountChanged(count);
        }
        return added;
    }

    public bool Unregister(EnemyStepper e)
    {
        if (e == null) return false;
        bool removed = _enemies.Remove(e);
        if (removed)
        {
            int count = _enemies.Count;
            if (OnEnemyCountChanged != null) OnEnemyCountChanged(count);
            if (count == 0 && OnAllEnemiesCleared != null) OnAllEnemiesCleared();
        }
        return removed;
    }

    void StepOnce()
    {
        // Current occupancy (enemies only; you can include the player if you want to forbid stepping onto them)
        var occupied = new HashSet<Vector2Int>(_enemies.Select(e => e.Agent.CurrentGridPos));
        var willVacate = new HashSet<Vector2Int>();   // tiles that earlier enemies will leave this tick
        var reservedDest = new HashSet<Vector2Int>(); // accepted destinations so far
        var reservedEdges = new HashSet<Edge>();      // for swap detection

        // Determine processing order
        var order = _enemies.Where(e => !e.Agent.IsMoving && e.Decision != null).ToList();
        if (order.Count == 0) return;

        if (shuffleOrderEachTick)
            order = order.OrderBy(_ => UnityEngine.Random.value).ToList();
        else
            order = order.OrderBy(e => e.GetInstanceID()).ToList().Rotate(_roundRobinOffset++); // see helper below

        foreach (var e in order)
        {
            var from = e.Agent.CurrentGridPos;

            // Primary choice
            var primary = e.Decision.DecideStep(e.Agent);
            if (!TryAccept(from, primary, e, occupied, willVacate, reservedDest, reservedEdges))
            {
                // Optional fallback (e.g., chase tries the other axis)
                if (!e.Decision.TryGetAlternate(e.Agent, primary, out var alt) ||
                    !TryAccept(from, alt, e, occupied, willVacate, reservedDest, reservedEdges))
                {
                    // wait this tick
                }
            }
        }
    }

    bool TryAccept(
        Vector2Int from, Vector2Int delta, EnemyStepper e,
        HashSet<Vector2Int> occupied, HashSet<Vector2Int> willVacate,
        HashSet<Vector2Int> reservedDest, HashSet<Edge> reservedEdges)
    {
        if (delta == Vector2Int.zero) return false;
        if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1) return false;

        var to = from + delta;
        if (!GridMath.IsWithinBounds(to)) return false;

        if (!LevelGrid.Instance.IsWalkable(to)) return false; // respect static walls

        // Already reserved by earlier enemy this tick?
        if (reservedDest.Contains(to)) return false;

        // Can we enter a tile that is currently occupied?
        bool isOccupiedNow = occupied.Contains(to);
        bool enteringVacated = allowEnterVacatedThisTick && willVacate.Contains(to);
        if (isOccupiedNow && !enteringVacated) return false;

        // Edge-swap prevention: if someone moves (to->from), block this move.
        if (blockSwaps && reservedEdges.Contains(new Edge(to, from))) return false;

        // Accept: reserve destination and edge, mark from as vacated, and start the move now.
        reservedDest.Add(to);
        reservedEdges.Add(new Edge(from, to));
        willVacate.Add(from);
        e.Agent.TryMove(delta);
        return true;
    }
}

static class ListExtensions
{
    public static List<T> Rotate<T>(this List<T> list, int offset)
    {
        if (list.Count == 0) return list;
        int n = Mathf.Abs(offset) % list.Count;
        if (n == 0) return list;
        var head = list.Skip(n).ToList();
        head.AddRange(list.Take(n));
        return head;
    }
}
