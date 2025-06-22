public static class MessageCounter
{
    public static event Action? MaxCountReached;
    public static event Action<string>? ProcessedResultReady;


    public static void OnMaxCountReached()
    {
        MaxCountReached?.Invoke();
    }

    public static void OnProcessedResultReady(String text)
    {
        ProcessedResultReady?.Invoke(text);
    }
}
