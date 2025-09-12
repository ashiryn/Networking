namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Class used to allow external classes to register to incoming network messages with an appropriate callback
/// </summary>
/// <typeparam name="TNetworkMessage">The type of network message the dispatcher is responsible for</typeparam>
/// <typeparam name="TNetworkEventArgs">The type of event args that the dispatcher is expecting to work with</typeparam>
public class NetworkMessageDispatcher<TNetworkMessage, TNetworkEventArgs>
    where TNetworkMessage : INetworkMessage
    where TNetworkEventArgs : INetworkMessageEventArgs<TNetworkMessage>
{
    /// <summary>
    ///     Queue used to store and free up network message handling quicker, items are dispatched during an update cycle
    /// </summary>
    protected Queue<TNetworkEventArgs> DispatchQueue { get; private set; } =
        new Queue<TNetworkEventArgs>();
    /// <summary>
    ///     Routing table for incoming message traffic to callbacks based on the Tag of a message
    /// </summary>
    protected Dictionary<ushort, Action<TNetworkEventArgs>?>
        MessageRoutingTable { get; private set; } =
        new Dictionary<ushort, Action<TNetworkEventArgs>?>();
    /// <summary>
    ///     Lock object to enforce thread safety
    /// </summary>
    protected Lock ThreadLock { get; set; } = new Lock();
    /// <summary>
    ///     Clears the routing table of all entries
    /// </summary>
    public void Clear()
    {
        lock (ThreadLock)
        {
            MessageRoutingTable.Clear();
        }
    }

    /// <summary>
    ///     Initializes the dispatcher for use
    /// </summary>
    public void Initialize()
    {
        ThreadLock = new Lock();
        MessageRoutingTable =
            new Dictionary<ushort, Action<TNetworkEventArgs>?>();

        DispatchQueue = new Queue<TNetworkEventArgs>();
    }
    /// <summary>
    ///     Prepares an event arg for dispatching according to thread safety rules
    /// </summary>
    /// <param name="e">The event arg to dispatch</param>
    public void PrepareForDispatch(TNetworkEventArgs e)
    {
        lock (ThreadLock)
        {
            DispatchQueue.Enqueue(e);
        }
    }
    /// <summary>
    ///     Registers a callback to a desired message tag id
    /// </summary>
    /// <param name="destination">The message tag id to register to</param>
    /// <param name="route">The callback to use when a message with the desired tag is received</param>
    public void RegisterMessageDestination(ushort destination,
                                           Action<TNetworkEventArgs> route)
    {
        lock (ThreadLock)
        {
            MessageRoutingTable.TryAdd(destination, null);
            MessageRoutingTable[destination] += route;
        }
    }
    /// <summary>
    ///     Removes a registered callback from the routing table
    /// </summary>
    /// <param name="destination">The message tag id to remove from</param>
    /// <param name="route">The callback to remove</param>
    public void RemoveMessageDestination(ushort destination,
                                         Action<TNetworkEventArgs> route)
    {
        lock (ThreadLock)
        {
            if (MessageRoutingTable.ContainsKey(destination))
            {
                MessageRoutingTable[destination] -= route;
            }
        }
    }
    /// <summary>
    ///     Removes an entire tag entry from the routing table
    /// </summary>
    /// <param name="destination">The message tag id to remove from the routing table</param>
    public void RemoveMessageDestination(ushort destination)
    {
        lock (ThreadLock)
        {
            if (MessageRoutingTable.ContainsKey(destination))
            {
                MessageRoutingTable[destination] = null;
            }

            MessageRoutingTable.Remove(destination);
        }
    }
    /// <summary>
    ///     Checks for any queued message events to dispatch, dispatching 1 per cycle if any exist
    /// </summary>
    public virtual void Update()
    {
        lock (ThreadLock)
        {
            if (!DispatchQueue.Any())
            {
                return;
            }

            TNetworkEventArgs currentDispatch = DispatchQueue.Dequeue();
            Dispatch(currentDispatch);
        }
    }
    /// <summary>
    ///     Helper function used to check for and fire callbacks for the incoming message event arg
    /// </summary>
    /// <param name="e">The event arg to dispatch out to all registered callbacks</param>
    protected virtual void Dispatch(TNetworkEventArgs e)
    {
        if (!MessageRoutingTable.ContainsKey(e.Tag) ||
            MessageRoutingTable[e.Tag] == null)
        {
            return;
        }

        MessageRoutingTable[e.Tag]?.Invoke(e);
    }
}