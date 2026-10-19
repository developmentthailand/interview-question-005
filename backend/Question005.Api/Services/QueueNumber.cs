namespace Example.Com.InterviewQuestion005.Services;

public static class QueueNumber
{
    public const int SequenceLength = 260;

    public static string FromSequence(int sequence)
    {
        if (sequence is < 0 or >= SequenceLength) throw new ArgumentOutOfRangeException(nameof(sequence));
        return $"{(char)('A' + sequence / 10)}{sequence % 10}";
    }
}
